using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using X360.STFS;
using System.Text.RegularExpressions;
using RocksmithToolkitLib.Extensions;
using RocksmithToolkitLib.DLCPackage.Manifest.Tone;
using RocksmithToolkitLib.DLCPackage.Manifest;
using RocksmithToolkitLib.PSARC;
using System.Xml.Serialization;
using RocksmithToolkitLib.DLCPackage.AggregateGraph;
using RocksmithToolkitLib.Ogg;


namespace RocksmithToolkitLib.DLCPackage
{
    public class DLCPackageData : IDisposable
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
        public bool IsOgg { get; set; }
        public Stream Audio
        {
            get { 
                if (_Audio == null)
                    ParseAudioFiles();
                return _Audio;
            }
            set { _Audio = value; }
        }
		public Stream AudioPreview{
            get { 
                if (_Audio == null)
                    ParseAudioFiles();
                return _Audio;
            }
            set { _Audio = value; }
        }
        public float Volume { get; set; }
        public int AverageTempo { get; set; }
        public PackageMagic SignatureType { get; set; }
        public string PackageVersion { get; set; }
		public List<Manifest.ChordTemplate> Chords { get; set; }
        public Platform Platform { get; set;}
        public AggregateGraph2014 AggregateGraph { get; set; }

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

        public float? PreviewVolume { get; set; }

        private PSARC.PSARC Archive;
        private Dictionary<object, Entry> _entries = new Dictionary<object, Entry>();
        private List<Arrangement> _Arrangements;
        private List<AlbumArt> _AlbumArt;
        private Stream _Audio;
        private Stream _AudioPreview;

        public DLCPackageData() {

        }
		public DLCPackageData(string filename, Platform targetPlatform = null) {
            
            this.BasePath = filename;
            this.Archive = new PSARC.PSARC();

            using (var fs = File.OpenRead(this.BasePath))
                this.Archive.Read(fs);

            this.SignatureType = PackageMagic.CON;
            this.Platform = targetPlatform ?? Packer.TryGetPlatformByEndName(filename);

            var aggregate = this.Archive.Entries.Single(e => e.Name.EndsWith("aggregategraph.nt"));
            this.AggregateGraph = AggregateGraph2014.LoadFromFile(aggregate.Data);
            _entries[this.AggregateGraph] = aggregate;

            var appid = this.Archive.Entries.Single(e => e.Name.Equals("/appid.appid"));
            using (var r = new StreamReader(appid.Data))
                this.AppId = r.ReadToEnd();
            _entries[this.AppId] = appid;

            ParseBaseData();
        }

        public List<Arrangement> Arrangements
        {
            get {
                if (this._Arrangements == null) {
                    this._Arrangements = new List<Arrangement>();
                    foreach (var json in this.AggregateGraph.JsonDB)
                    {
                        var jsonEntry = Archive.Entries.Single(e => e.Name.Equals(json.RelPath));
                        var attr = Manifest2014<Attributes2014>.LoadFromFile(jsonEntry.Data).Entries.ToArray()[0].Value.ToArray()[0].Value;

                        var sngFile = AggregateGraph.MusicgameSong.Where(s => s.Name.Equals(json.Name))
                                           .Concat(AggregateGraph.SongXml.Where(x => x.Name.Equals(json.Name)))
                                           .First();
                        var sngEntry = Archive.Entries.Single(e => e.Name.Equals(sngFile.RelPath));

                        var arr = Arrangement.Read(attr, Platform, sngEntry.Name, sngEntry.Data);
                        this._entries[arr] = jsonEntry;
                        this._entries[arr.Sng2014] = sngEntry;

                        this._Arrangements.Add(arr);
                    }
                }
                return this._Arrangements;
            }
        }

        public List<Tone2014> TonesRS2014
        {
            get
            {
                return this.Arrangements.SelectMany(a => a.Tones).Distinct(new PropertyComparer<Tone2014>("Key")).ToList();
            }
        }

        public List<AlbumArt> AlbumArt
        {
            get
            {
                if (_AlbumArt == null)
                {
                    _AlbumArt = AggregateGraph.ImageArt.Select(ia => new AlbumArt(ia.getEntry(Archive).Data)).ToList();
                }
                return _AlbumArt;
            }
        }

        private string _AudioFile;

        public string TempAudioFile
        {
            get
            {
                if (_AudioFile == null)
                {
                    _AudioFile = GeneralExtensions.GetTempFileName(".ogg");
                    using (var fs = File.OpenWrite(_AudioFile))
                        Audio.CopyTo(fs);
                }
                return _AudioFile;
            }
        }
        private void ParseAudioFiles()
        {
            var oggFiles = Archive.Entries.Where(e => e.Name.EndsWith(".ogg")).OrderByDescending(e => e.Data.Length);
            if (oggFiles.Count() > 0)
            {
                Audio = oggFiles.First().Data;
                if (oggFiles.Count() > 1)
                    _AudioPreview = oggFiles.Last().Data;
                return;
            }

            var wemFiles = Archive.Entries.Where(e => e.Name.EndsWith(".wem")).OrderByDescending(e => e.Data.Length);
            if (oggFiles.Count() > 0)
            {
                _Audio = OggFile.ConvertOgg(oggFiles.First().Data);
                if (oggFiles.Count() > 1)
                    _AudioPreview = OggFile.ConvertOgg(oggFiles.Last().Data);
                return;
            }

            throw new InvalidDataException("Audio files not found.");
        }

		private void ParseBaseData ()
		{
            var attr = AggregateGraph.JsonDB.Select(j => j.getManifest(Archive)).First(m => m.SongName != null);
			this.Name = attr.DLCKey;
			this.SongInfo = new SongInfo ();
			this.SongInfo.SongDisplayName = attr.SongName;
			this.SongInfo.SongDisplayNameSort = attr.SongNameSort;
            this.SongInfo.Album = attr.AlbumName;
            this.SongInfo.AlbumSort = attr.AlbumNameSort;
            this.SongInfo.SongYear = attr.SongYear ?? 0;
			this.SongInfo.Artist = attr.ArtistName;
			this.SongInfo.ArtistSort = attr.ArtistNameSort;
			this.SongInfo.AverageTempo = (int)attr.SongAverageTempo;
            
            // FIXME these should aggregate
            this.Volume = attr.SongVolume;
            this.PreviewVolume = (attr.PreviewVolume != null) ? (float)attr.PreviewVolume : this.Volume;
            this.Difficulty = attr.SongDifficulty;
			this.Chords = attr.ChordTemplates;
            this.SongLength = attr.SongLength ?? 0;
        }

        public IDictionary<string, Entry> OtherEntries { get { 
            return Archive.Entries.ToDictionary(e => e.Name, e => e);  
        } } // TODO -- filter to unrecognized ones

        #endregion

        #region RS2014 Inlay only

        [XmlIgnore]
        public InlayData Inlay { get; set; }

        #endregion

        // needs to be called after all packages for platforms are created
        public void CleanCache() {
            
            if (Arrangements != null)
                foreach (var a in Arrangements)
                    a.CleanCache();

            if (_AudioFile != null)
            {
                File.Delete(_AudioFile);
                _AudioFile = null;
            }
        }

        public void Dispose()
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

        public Stream Showlights
        {
            get
            {
                var showlights = this.Archive.Entries.SingleOrDefault(e => e.Name.Equals(AggregateGraph.ShowlightXml.RelPath));
                if (showlights == null)
                    return null;

                _entries[showlights.Data] = showlights;
                return showlights.Data;
            }
 }

        public Stream LyricsTex
        {
            get
            {
                var lyrics = this.Archive.Entries.SingleOrDefault(e => e.Name.Equals(AggregateGraph.LyricsTex.RelPath));
                if (lyrics == null)
                    return null;

                _entries[lyrics.Data] = lyrics;
                return lyrics.Data;
            }
        }

        public string BasePath { get; private set; }

        public string Digest { get { return Archive.Digest; } }
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

    public static class HelperExtensions
    {
        public static Entry getEntry(this GraphItem gi, PSARC.PSARC archive)
        {
            return archive.Entries.Single(e => e.Name.Equals(gi.RelPath));
        }

        public static Attributes2014 getManifest(this GraphItem jsonItem, PSARC.PSARC archive)
        {
            return Manifest2014<Attributes2014>.LoadFromFile(jsonItem.getEntry(archive).Data).Entries.ToArray()[0].Value.ToArray()[0].Value;
        }

    }
}