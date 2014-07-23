using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using X360.STFS;
using System.Text.RegularExpressions;
using RocksmithToolkitLib.Extensions;
using RocksmithToolkitLib.DLCPackage.Manifest.Tone;
using RocksmithToolkitLib.DLCPackage.Manifest;
using RocksmithToolkitLib.DLCPackage.AggregateGraph;
using RocksmithToolkitLib.Sng;
using RocksmithToolkitLib.Ogg;
using RocksmithToolkitLib.PSARC;
using System.Xml.Serialization;

using System.Drawing;
using System.Drawing.Imaging;
using RocksmithToolkitLib.Sng2014HSL;
using RocksmithToolkitLib.Xml;

namespace RocksmithToolkitLib.DLCPackage
{
    public class DLCPackageData
    {
        public GameVersion GameVersion;
        
        public bool Pc { get; set; }
        public bool Mac { get; set; }
        public bool XBox360 { get; set; }
        public bool PS3 { get; set; }

        public double? SongLength { get; set; }
        public string AppId { get; set; }
        public string Name { get; set; }
        public SongInfo SongInfo { get; set; }
        public string AlbumArtPath { get; set; }
		public bool IsOgg { get; set; }
		public string AudioPath { get; set; }
		public string AudioPreviewPath { get; set; }
        public List<Arrangement> Arrangements { get; set; }
        public float Volume { get; set; }
        public PackageMagic SignatureType { get; set; }
        public string PackageVersion { get; set; }
		public List<Manifest.ChordTemplate> Chords { get; set; }
        public Platform targetPlatform { get; set;}
        private List<XBox360License> xbox360Licenses = null;
        public List<XBox360License> XBox360Licenses
        {
            get
            {
                if (xbox360Licenses == null)
                {
                    xbox360Licenses = new List<XBox360License>();
                    return xbox360Licenses;
                }
                else
                    return xbox360Licenses;
            }
            set { xbox360Licenses = value; }
        }

        #region RS1 only

        public List<Tone.Tone> Tones { get; set; }

        #endregion

        #region RS2014 only

        public List<Tone2014> TonesRS2014 { get; set; }
        public float? PreviewVolume { get; set; }
        
        // Cache art image conversion
        public List<DDSConvertedFile> ArtFiles { get; set; }

        public string LyricsTex { get; set; }

		private class FileEntry {
			public String Path { get; set; }
			public Stream Data { get; set; }
			public FileEntry (String path, Stream data) {
				Path = path;
				Data = data;
			}
		}
		public static DLCPackageData Load(string unpackedDirOrPsarc, Platform targetPlatform) {
            //Load files
			var files = new Dictionary<string, FileEntry> ();
			if (unpackedDirOrPsarc.EndsWith(".psarc")) {
				var archive = new PSARC.PSARC();
				using (FileStream fileStream = File.OpenRead(unpackedDirOrPsarc))
				{
					archive.Read((Stream) fileStream);
				}
				files = archive.Entries.ToDictionary( e => Path.GetFileName(e.Name), e => new FileEntry(e.Name, e.Data));

                // TEMPORARY HACK -- dds uses a separate process so we need to save these files somewhere for it to get to them
                foreach (var ddsFile in files.Where(f => f.Key.EndsWith(".dds")).Select(kv => kv.Value)) {
                    using (var tempFile = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 4096))
                    {
                        ddsFile.Data.CopyTo(tempFile);
                        ddsFile.Path = tempFile.Name;
                    }
                }
			} else {
				files = Directory.GetFiles(unpackedDirOrPsarc, "*.*", SearchOption.AllDirectories).ToDictionary(f => Path.GetFileName(f), f => new FileEntry( f, File.OpenRead(f)));
			}

            var data = new DLCPackageData();
            data.SignatureType = PackageMagic.CON;
            data.targetPlatform = targetPlatform;

			//Get Arrangements / Tones
            data.Arrangements = new List<Arrangement>();
            data.TonesRS2014 = new List<Tone2014>();

			foreach (var json in files.Where(kv => kv.Key.EndsWith( ".json")).Select(kv => kv.Value.Data)) {
				Attributes2014 attr = Manifest2014<Attributes2014>.LoadFromFile(json).Entries.ToArray()[0].Value.ToArray()[0].Value;
				FileEntry xmlFile = null;
				files.TryGetValue (attr.SongXml.Split(':')[3] + ".xml", out xmlFile);
                FileEntry sngFile = null;
                files.TryGetValue(attr.SongXml.Split(':')[3] + ".sng", out sngFile);

				AddArrangement (data, attr, xmlFile, sngFile);
            }

            //Get Files
			var art = files.FirstOrDefault (kv => kv.Key.EndsWith ("_256.dds")).Value;
			if (art != null) {
				data.AlbumArtPath = art.Path; 
			}
			var targetAudioFiles = files.Where (kv => kv.Key.EndsWith (".wem")).OrderByDescending(kv => ((FileStream)kv.Value.Data).Length).Select(kv => kv.Key); // FIXME(jtk) don't really want to have to assume FileStream here
//                var newFile = Path.Combine(Path.GetDirectoryName(file), String.Format("{0}_fixed{1}", Path.GetFileNameWithoutExtension(file), Path.GetExtension(file)));
//                if (targetPlatform.IsConsole != file.GetAudioPlatform().IsConsole)
//                {
//                    OggFile.ConvertAudioPlatform(file, newFile);
//                    targetAudioFiles.Add(newFile);
//                }
//                else 
//					targetAudioFiles.Add(file);
//            }

            if (targetAudioFiles.Count() <= 0)
                throw new InvalidDataException("Audio files not found.");

			data.AudioPath = targetAudioFiles.First();
			data.AudioPreviewPath = targetAudioFiles.LastOrDefault ();
            //Make Audio preview with expected name when rebuild
			// FIXME(jtk) -- move to somewhere else
//            if (!String.IsNullOrEmpty(audioPreviewPath)) {
//                var newPreviewFileName = Path.Combine(Path.GetDirectoryName(audioPath), String.Format("{0}_preview{1}", Path.GetFileNameWithoutExtension(audioPath), Path.GetExtension(audioPath)));
//                File.Move(audioPreviewPath, newPreviewFileName);
//				data.AudioPreviewPath = newPreviewFileName;
//            }

			data.AppId = files.FirstOrDefault(kv => kv.Key.StartsWith(".appid")).Key; // FIXME(jtk) -- get data not file
            return data;
        }

		private static void AddArrangement (DLCPackageData data , Attributes2014 attr, FileEntry xmlFile, FileEntry sngFile)
		{
			if (attr.Phrases != null) {
				if (data.SongInfo == null) {
					// Fill Package Data
					data.Name = attr.DLCKey;
					data.Volume = attr.SongVolume;
					data.PreviewVolume = (attr.PreviewVolume != null) ? (float)attr.PreviewVolume : data.Volume;
					// Fill SongInfo
					data.SongInfo = new SongInfo ();
					data.SongInfo.SongDisplayName = attr.SongName;
					data.SongInfo.SongDisplayNameSort = attr.SongNameSort;
					data.SongInfo.Album = attr.AlbumName;
					data.SongInfo.SongYear = attr.SongYear ?? 0;
					data.SongInfo.Artist = attr.ArtistName;
					data.SongInfo.ArtistSort = attr.ArtistNameSort;
					data.SongInfo.AverageTempo = (int)attr.SongAverageTempo;
					data.Difficulty = attr.SongDifficulty;
					data.Chords = attr.ChordTemplates;
                    data.SongLength = attr.SongLength;

				}
				// Adding Tones
				foreach (var jsonTone in attr.Tones) {
					if (jsonTone == null)
						continue;
					if (!data.TonesRS2014.OfType<Tone2014> ().Any (t => t.Key == jsonTone.Key))
						data.TonesRS2014.Add (jsonTone);
				}
                if (sngFile != null)
                {
                    data.Arrangements.Add(new Arrangement(attr, Sng2014File.ReadSng(sngFile.Data, data.targetPlatform)));
                }
				else if (xmlFile != null) {
					// Adding Arrangement
					data.Arrangements.Add (new Arrangement (attr, xmlFile.Path, xmlFile.Data)); // FIXME -- we wont have a song
                }
                
			}
			else {
				var voc = new Arrangement ();
				voc.Name = ArrangementName.Vocals;
				voc.ArrangementType = ArrangementType.Vocal;
				voc.SongXml = new SongXML {
					File = (xmlFile == null ? "" : xmlFile.Path)
				};
				voc.SongFile = new SongFile {
					File = ""
				};
				if (xmlFile != null) {
					voc.Sng2014 = Sng2014HSL.Sng2014FileWriter.ReadVocals(xmlFile.Data);
				}
				voc.ScrollSpeed = 20;
				// Adding Arrangement
				data.Arrangements.Add (voc);
			}
		}

        #endregion

        #region RS2014 Inlay only

        [XmlIgnore]
        public InlayData Inlay { get; set; }

        #endregion

        // needs to be called after all packages for platforms are created
        public void CleanCache() {
            if (ArtFiles != null) {
                foreach (var file in ArtFiles) {
                    try {
                        File.Delete(file.destinationFile);
                    } catch { }
                }
                ArtFiles = null;
            }

            if (Arrangements != null)
                foreach (var a in Arrangements)
                    a.CleanCache();
        }

        ~DLCPackageData()
        {
            CleanCache();
        }

        public static string DoLikeProject(string unpackedDir)
        {
            //Get name for new folder name
            string outdir = "";
            string EOF = "EOF";
            string KIT = "Toolkit";
            string SongName = "SongName";
            var jsonFiles = Directory.GetFiles(unpackedDir, "*.json", SearchOption.AllDirectories);
            var attr = Manifest2014<Attributes2014>.LoadFromFile(jsonFiles[0]).Entries.ToArray()[0].Value.ToArray()[0].Value;

            //Create dir sruct
            SongName = attr.FullName.Split('_')[0];
            outdir = Path.Combine(Path.GetDirectoryName(unpackedDir), String.Format("{0}_{1}", attr.ArtistNameSort.GetValidName(false), attr.SongNameSort.GetValidName(false)));
            if (Directory.Exists(outdir))
                outdir += "_" + DateTime.Now.ToString("yyyy-MM-dd");

            Directory.CreateDirectory(outdir);
            Directory.CreateDirectory(Path.Combine(outdir, EOF));
            Directory.CreateDirectory(Path.Combine(outdir, KIT));

            foreach (var json in jsonFiles)
            {
                var atr = Manifest2014<Attributes2014>.LoadFromFile(json).Entries.ToArray()[0].Value.ToArray()[0].Value;
                var Name = atr.SongXml.Split(':')[3];
                var xmlFile = Directory.GetFiles(unpackedDir, Name + ".xml", SearchOption.AllDirectories)[0];

                //Move all pair JSON\XML
                File.Move(json, Path.Combine(outdir, KIT, Name + ".json"));
                File.Move(xmlFile, Path.Combine(outdir, EOF, Name + ".xml"));
            }

            //Move art_256.dds to KIT folder
            var ArtFile = Directory.GetFiles(unpackedDir, "*_256.dds", SearchOption.AllDirectories);
            if (ArtFile.Length > 0)
                File.Move(ArtFile[0], Path.Combine(outdir, KIT, Path.GetFileName(ArtFile[0])));

            //Move ogg to EOF folder + rename
            var OggFiles = Directory.GetFiles(unpackedDir, "*_fixed.ogg", SearchOption.AllDirectories);
            if(OggFiles.Count() <= 0)
                throw new InvalidDataException("Audio files not found.");

            var a0 = new FileInfo(OggFiles[0]);
            FileInfo b0 = null;
            if (OggFiles.Count() == 2){
                b0 = new FileInfo(OggFiles[1]);

                if (a0.Length > b0.Length) {
                    File.Move(a0.FullName, Path.Combine(outdir, EOF, SongName + ".ogg"));
                    File.Move(b0.FullName, Path.Combine(outdir, EOF, SongName + "_preview.ogg"));
                } else {
                    File.Move(b0.FullName, Path.Combine(outdir, EOF, SongName + ".ogg"));
                    File.Move(a0.FullName, Path.Combine(outdir, EOF, SongName + "_preview.ogg"));
                }
            }
            else File.Move(a0.FullName, Path.Combine(outdir, EOF, SongName + ".ogg"));

            //Move wem to KIT folder + rename
            var WemFiles = Directory.GetFiles(unpackedDir, "*.wem", SearchOption.AllDirectories);
            if(WemFiles.Count() <= 0)
                throw new InvalidDataException("Audio files not found.");

            var a1 = new FileInfo(WemFiles[0]);
            FileInfo b1 = null;
            if (WemFiles.Count() == 2){
                b1 = new FileInfo(WemFiles[1]);

                if (a1.Length > b1.Length) {
                    File.Move(a1.FullName, Path.Combine(outdir, KIT, SongName + ".wem"));
                    File.Move(b1.FullName, Path.Combine(outdir, KIT, SongName + "_preview.wem"));
                } else {
                    File.Move(b1.FullName, Path.Combine(outdir, KIT, SongName + ".wem"));
                    File.Move(a1.FullName, Path.Combine(outdir, KIT, SongName + "_preview.wem"));
                }
            } 
            else File.Move(a1.FullName, Path.Combine(outdir, KIT, SongName + ".wem"));

            //Move Appid for correct template generation.
            var appidFile = Directory.GetFiles(unpackedDir, "*.appid", SearchOption.AllDirectories);
            if (appidFile.Length > 0)
                File.Move(appidFile[0], Path.Combine(outdir, KIT, Path.GetFileName(appidFile[0])));

            //Remove old folder
            DirectoryExtension.SafeDelete(unpackedDir);

            return outdir;
        }

        public double? Difficulty { get; set; }
    }

    public class DDSConvertedFile {
        public int sizeX { get; set; }
        public int sizeY { get; set; }
        public string sourceFile { get; set; }
        public string destinationFile { get; set; }
    }

    public class InlayData {
        public string DLCSixName { get; set; }
        public string InlayPath { get; set; }
        public string IconPath { get; set; }
        public Guid Id { get; set; }
        public bool Frets24 { get; set; }
        public bool Colored { get; set; }

        public InlayData() {
            Id = IdGenerator.Guid();
        }
    }
}