using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using RocksmithToolkitLib.DLCPackage;
using RocksmithToolkitLib.DLCPackage.AggregateGraph;
using RocksmithToolkitLib.DLCPackage.Manifest;
using RocksmithToolkitLib.Sng;
using RocksmithToolkitLib.Sng2014HSL;
using RocksmithToolkitLib.Xml;
using RocksmithToolkitLib.Extensions;
using RocksmithToolkitLib.DLCPackage.Manifest.Tone;

namespace RocksmithToolkitLib.DLCPackage
{
    public enum RouteMask : int {
        // Used for lessons or for display only in song list
        None = 0,
        Lead = 1,
        Rhythm = 2,
        Any = 3,
        Bass = 4
    }

    public enum DNAId : int {
        None = 0,
        Solo = 1,
        Riff = 2,
        Chord = 3
    }

    public class Arrangement
    {
        private string _songFilename = null;

        public SongFile SongFile
        {
            get
            {
                var platform = new Platform(GamePlatform.Pc, GameVersion.RS2014); //FIXME
                var stream = new MemoryStream();
                Sng2014.WriteSng(stream, platform);
                return new SongFile() { File = Path.ChangeExtension(_songFilename, ".sng"), Data = stream };               
            }            
        }

        public SongXML SongXml
        {
            get
            {
                var platform = new Platform(GamePlatform.Pc, GameVersion.RS2014); //FIXME
                var stream = new MemoryStream();
                dynamic xmlContent = null;
                if (this.ArrangementType == ArrangementType.Vocal)
                    xmlContent = new Vocals(this.Sng2014);
                else
                    xmlContent = new Song2014(this.Sng2014, null);
                xmlContent.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return new SongXML() { File = Path.ChangeExtension(_songFilename, ".xml"), Data = stream };    
               
            }
        }

        // Song Information
        public int ArrangementSort { get; set; }
        public ArrangementName Name { get; private set; }
        public string Tuning { get; set; }
        public TuningStrings TuningStrings { get; set; }
        public double TuningPitch { get; set; }
        public int ScrollSpeed { get; set; }
        public PluckedType PluckedType { get; set; }
        // cache parsing results (speeds up generating for multiple platforms)
        public Sng2014File Sng2014 { get; private set; }
        // Gameplay Path
        public RouteMask RouteMask { get; set; }
        public bool BonusArr = false;

        // Tone Selector
        public string ToneBase { get; set; }
        public string ToneMultiplayer { get; set; }
        public string ToneA { get; set; }
        public string ToneB { get; set; }
        public string ToneC { get; set; }
        public string ToneD { get; set; }

        public List<Tone2014> Tones { get; set; }

        // DLC ID
        public Guid Id { get; set; }
        public int MasterId { get; set; }

        public static Arrangement Read(Attributes2014 attr, Platform platform, Guid id, string filename, Stream data = null)
        {
            Arrangement result = null;
            using (var str = data ?? File.OpenRead(filename))
            {
                switch (Path.GetExtension(filename))
                {
                    case ".xml":
                        Sng2014File xml = null;
                        if (((ArrangementName)attr.ArrangementType) == ArrangementName.Vocals)
                            xml = Sng2014FileWriter.ReadVocals(data);
                        else
                            xml = Sng2014File.ConvertXML(str);
                        result = new Arrangement(attr, xml, id);
                        break;
                    case ".sng":
                        result = new Arrangement(attr, Sng2014File.ReadSng(str, platform), id);
                        break;
                    default:
                        throw new Exception("Unknown file type: " + filename);
                }
                result._songFilename = filename;
                        
            }
            return result;
        }
        
        public ArrangementType ArrangementType
        {
            get
            {
                switch (Name)
                {
                    case ArrangementName.Bass:
                        return Sng.ArrangementType.Bass;
                    case ArrangementName.Vocals:
                        return Sng.ArrangementType.Vocal;
                    default:
                        return Sng.ArrangementType.Guitar;
                }
            }
        }

        private Arrangement(Attributes2014 attr, Sng2014File song, Guid id) {
            this.ArrangementSort = attr.ArrangementSort;
            this.Sng2014 = song;
            this.Name = (ArrangementName)Enum.Parse(typeof(ArrangementName), attr.ArrangementName);
            this.ScrollSpeed = 20;
            this.Id = id;
            this.MasterId = ArrangementType == Sng.ArrangementType.Vocal ? 1 : RandomGenerator.NextInt();

            if (this.ArrangementType == ArrangementType.Vocal)
            {
                this.Tones = new List<Tone2014>();
            } else
            {
                ParseTuning(this, attr);
                ParseTones(this, attr);
            }
        }

        private static void ParseTones(Arrangement dest, Attributes2014 attr)
        {
            dest.ScrollSpeed = Convert.ToInt32(attr.DynamicVisualDensity.Last() * 10);
            dest.PluckedType = (PluckedType)attr.ArrangementProperties.BassPick;
            dest.RouteMask = (RouteMask)attr.ArrangementProperties.RouteMask;
            dest.BonusArr = attr.ArrangementProperties.BonusArr == 1;
            dest.ToneBase = attr.Tone_Base;
            dest.ToneMultiplayer = attr.Tone_Multiplayer;
            dest.ToneA = attr.Tone_A;
            dest.ToneB = attr.Tone_B;
            dest.ToneC = attr.Tone_C;
            dest.ToneD = attr.Tone_D;
            dest.Tones = attr.Tones.ToList();
        }

        private static void ParseTuning(Arrangement dest, Attributes2014 attr)
        {
            bool isBass = false;
            TuningDefinition tuning = null;
            switch (dest.Name)
            {
                case ArrangementName.Bass:
                    tuning = TuningDefinitionRepository.Instance().SelectForBass(attr.Tuning, GameVersion.RS2014);
                    isBass = true;
                    break;
                case ArrangementName.Vocals:
                    break;
                default:
                    tuning = TuningDefinitionRepository.Instance().Select(attr.Tuning, GameVersion.RS2014);
                    break;

            }

            if (tuning == null)
            {
                tuning = new TuningDefinition();
                tuning.UIName = tuning.Name = TuningDefinition.NameFromStrings(attr.Tuning, isBass);
                tuning.Custom = true;
                tuning.GameVersion = GameVersion.RS2014;
                tuning.Tuning = attr.Tuning;
                TuningDefinitionRepository.Instance().Add(tuning, true);
            }
            dest.Tuning = tuning.UIName;
            dest.TuningStrings = tuning.Tuning;

            if (attr.CentOffset != null)
                dest.TuningPitch = attr.CentOffset.Cents2Frequency();
        }

        public override string ToString()
        {
            var toneDesc = String.Empty;
            if (!String.IsNullOrEmpty(ToneBase))
                toneDesc = ToneBase;
            if (!String.IsNullOrEmpty(ToneB))
                toneDesc += String.Format(", {0}", ToneB);
            if (!String.IsNullOrEmpty(ToneC))
                toneDesc += String.Format(", {0}", ToneC);
            if (!String.IsNullOrEmpty(ToneD))
                toneDesc += String.Format(", {0}", ToneD);

            switch (ArrangementType)
            {
                case ArrangementType.Bass:
                    return String.Format("{0} [{1}] ({2})", ArrangementType, Tuning, toneDesc);
                case ArrangementType.Vocal:
                    return String.Format("{0}", ArrangementType);
                default:
                    return String.Format("{0} - {1} [{2}] ({3})", ArrangementType, Name, Tuning, toneDesc);
            }
        }

        public void CleanCache()
        {
            Sng2014 = null;
        }


        public string SongFilePath { get { return Path.ChangeExtension(this._songFilename, ".sng"); } }
    }
}
