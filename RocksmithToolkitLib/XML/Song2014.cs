using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using Newtonsoft.Json;
using RocksmithToolkitLib.Extensions;
using RocksmithToolkitLib.DLCPackage.Manifest;
using RocksmithToolkitLib.DLCPackage.Manifest.Tone;
using RocksmithToolkitLib.Sng;

namespace RocksmithToolkitLib.Xml {
    [XmlRoot("song", Namespace = "", IsNullable = false)]
    public class Song2014 {
        [XmlAttribute("version")] // RS2014 is 7 or above
        public string Version { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("arrangement")]
        public string Arrangement { get; set; }

        [XmlElement("part")]
        public Int16 Part { get; set; }

        [XmlElement("offset")]
        public Single Offset { get; set; }

        [XmlElement("centOffset")]
        public string CentOffset { get; set; }

        [XmlElement("songLength")]
        public Single SongLength { get; set; }

        [XmlElement("songNameSort")]
        public string SongNameSort { get; set; }

        [XmlElement("startBeat")]
        public Single StartBeat { get; set; }

        [XmlElement("averageTempo")]
        public Single AverageTempo { get; set; }

        [XmlElement("tuning")]
        public TuningStrings Tuning { get; set; }

        [XmlElement("capo")]
        public Byte Capo { get; set; }
        
        [XmlElement("artistName")]
        public string ArtistName { get; set; }

        [XmlElement("artistNameSort")]
        public string ArtistNameSort { get; set; }

        [XmlElement("albumName")]
        public string AlbumName { get; set; }

        [XmlElement("albumNameSort")]
        public string AlbumNameSort { get; set; }

        [XmlElement("albumYear")]
        public string AlbumYear { get; set; }

        [XmlElement("albumArt")]
        public string AlbumArt { get; set; }

        [XmlElement("crowdSpeed")]
        public string CrowdSpeed { get; set; }

        [XmlElement("arrangementProperties")]
        public SongArrangementProperties2014 ArrangementProperties { get; set; }

        [XmlElement("lastConversionDateTime")]
        public string LastConversionDateTime { get; set; }

        [XmlArray("phrases")]
        [XmlArrayItem("phrase")]
        public SongPhrase[] Phrases { get; set; }

        [XmlArray("newLinkedDiffs")]
        [XmlArrayItem("newLinkedDiff")]
        public SongNewLinkedDiff[] NewLinkedDiff { get; set; }

        [XmlArray("phraseIterations")]
        [XmlArrayItem("phraseIteration", typeof(SongPhraseIteration), Type = typeof(SongPhraseIteration2014))]
        public SongPhraseIteration2014[] PhraseIterations { get; set; }

        [XmlArray("linkedDiffs")]
        [XmlArrayItem("linkedDiff")]
        public SongLinkedDiff[] LinkedDiffs { get; set; }

        [XmlArray("phraseProperties")]
        [XmlArrayItem("phraseProperty")]
        public SongPhraseProperty[] PhraseProperties { get; set; }

        [XmlArray("chordTemplates")]
        [XmlArrayItem("chordTemplate")]
        public SongChordTemplate2014[] ChordTemplates { get; set; }

        [XmlArray("fretHandMuteTemplates")]
        [XmlArrayItem("fretHandMuteTemplate")]
        public SongFretHandMuteTemplate[] FretHandMuteTemplates { get; set; }

        [XmlArray("ebeats")]
        [XmlArrayItem("ebeat")]
        public SongEbeat[] Ebeats { get; set; }

        [XmlElement("tonebase")]
        public string ToneBase { get; set; }

        [XmlElement("tonea")]
        public string ToneA { get; set; }

        [XmlElement("toneb")]
        public string ToneB { get; set; }

        [XmlElement("tonec")]
        public string ToneC { get; set; }

        [XmlElement("toned")]
        public string ToneD { get; set; }

        [XmlArray("tones")]
        [XmlArrayItem("tone")]
        public SongTone2014[] Tones { get; set; }

        [XmlArray("sections")]
        [XmlArrayItem("section")]
        public SongSection[] Sections { get; set; }

        [XmlArray("events")]
        [XmlArrayItem("event")]
        public SongEvent[] Events { get; set; }

        [XmlArray("controls")]
        [XmlArrayItem("control")]
        public SongControl[] Controls { get; set; }

        [XmlArray("levels")]
        [XmlArrayItem("level", typeof(SongLevel2014))]
        public SongLevel2014[] Levels { get; set; }

        public Song2014() { }

        public Song2014(Sng2014HSL.Sng sngData, Attributes2014 attr = null) {
            Version = "7";
            CrowdSpeed = "1";

            if (attr != null) {
                // If manifest is passed, fill general song information
                Title = attr.SongName;
                Arrangement = ((ArrangementName)attr.ArrangementType).ToString();
                Part = (short)attr.SongPartition;
                Offset = attr.SongOffset;
                CentOffset = Convert.ToString(attr.CentOffset);
                SongLength = (float)attr.SongLength;
                SongNameSort = attr.SongNameSort;
                AverageTempo = attr.SongAverageTempo;
                Tuning = attr.Tuning;
                Capo = Convert.ToByte(attr.CapoFret);
                ArtistName = attr.ArtistName;
                ArtistNameSort = attr.ArtistNameSort;
                AlbumName = attr.AlbumName;
                AlbumNameSort = attr.AlbumNameSort;
                AlbumYear = Convert.ToString(attr.SongYear) ?? "";
                AlbumArt = attr.AlbumArt;                
                ArrangementProperties = attr.ArrangementProperties;
                LastConversionDateTime = attr.LastConversionDateTime;

                ToneBase = attr.Tone_Base;
                ToneA = attr.Tone_A;
                ToneB = attr.Tone_B;
                ToneC = attr.Tone_C;
                ToneD = attr.Tone_D;
            } else {
                Part = sngData.Metadata.Part;
                SongLength = sngData.Metadata.SongLength;
                Tuning = new TuningStrings(sngData.Metadata.Tuning);
                Capo = (sngData.Metadata.CapoFretId >= 0) ? sngData.Metadata.CapoFretId : (byte)0;
                LastConversionDateTime = sngData.Metadata.LastConversionDateTime.ToNullTerminatedAscii();
                ArrangementProperties = new SongArrangementProperties2014();
            }

            Tones = (attr != null) ? SongTone2014.Parse(sngData.Tones, attr) : SongTone2014.Parse(sngData.Tones);
            if (attr == null) { // Fix tones slots for fake tone names if manifest was not entered
                foreach (var tone in Tones) {
                    if (tone.Name.EndsWith("_0"))
                        ToneBase = tone.Name;
                    if (tone.Name.EndsWith("_1")) {
                        ToneA = ToneBase;
                        ToneB = tone.Name;
                    }
                    if (tone.Name.EndsWith("_2"))
                        ToneC = tone.Name;
                    if (tone.Name.EndsWith("_3"))
                        ToneD = tone.Name;
                }
            }
            
            //Sections can be obtained from manifest or sng file (manifest preferred)
            Sections = (attr != null) ? SongSection.Parse(attr.Sections) : SongSection.Parse(sngData.Sections);

            //Can be obtained from manifest or sng file (sng preferred)
            Phrases = SongPhrase.Parse(sngData.Phrases);
            PhraseIterations = SongPhraseIteration2014.Parse(sngData.PhraseIterations);

            //Can be obtained from manifest or sng file (combined preferred)
            ChordTemplates = SongChordTemplate2014.Parse(sngData.Chords); // Only SNG have all ChordTemplates, manifest have only chord templates with name
            if (attr != null)
            {
                SongChordTemplate2014.AddChordIds(ChordTemplates, attr.ChordTemplates); // Only manifest has chordIds
            }

            //Only in SNG
            Ebeats = SongEbeat.Parse(sngData.BPMs);
            StartBeat = sngData.BPMs.BPMs[0].Time;
            Events = SongEvent.Parse(sngData.Events);
            Levels = SongLevel2014.Parse(sngData);

            //Not used in RS2014 customs at this time. Need to check official files
            NewLinkedDiff = SongNewLinkedDiff.Parse(sngData.NLD);
            PhraseProperties = SongPhraseProperty.Parse(sngData.PhraseExtraInfo);
            LinkedDiffs = new SongLinkedDiff[0];
            FretHandMuteTemplates = new SongFretHandMuteTemplate[0];
        }

        public static Song2014 LoadFromFile(string xmlSongRS2014File) {
			return LoadFromFile(File.OpenRead(xmlSongRS2014File));
		}

		public static Song2014 LoadFromFile(Stream xmlSongRS2014Stream) {
            xmlSongRS2014Stream.Seek(0, SeekOrigin.Begin);
			using (var reader = new StreamReader(xmlSongRS2014Stream))
            {
                return new XmlStreamingDeserializer<Song2014>(reader).Deserialize();
            }
        }

        public void Serialize(Stream stream) {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            using (var writer = XmlWriter.Create(stream, new XmlWriterSettings {
                Indent = true,
                OmitXmlDeclaration = true
            })) {
                new XmlSerializer(typeof(Song2014)).Serialize(writer, this, ns);
            }

            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
        }
    }

    public class SongArrangementProperties2014 : SongArrangementProperties {
        [JsonProperty("bonusArr")]
        [XmlAttribute("bonusArr")]
        public Int32 BonusArr { get; set; }

        [JsonProperty("pathLead")]
        [XmlAttribute("pathLead")]
        public Int32 PathLead { get; set; }

        [JsonProperty("pathRhythm")]
        [XmlAttribute("pathRhythm")]
        public Int32 PathRhythm { get; set; }

        [JsonProperty("pathBass")]
        [XmlAttribute("pathBass")]
        public Int32 PathBass { get; set; }

        [JsonProperty("routeMask")]
        [XmlAttribute("routeMask")]
        public Int32 RouteMask { get; set; }
    }

    public class SongPhraseIteration2014 : SongPhraseIteration {
        [XmlAttribute("variation")]
        public string Variation { get; set; }

        [XmlArray("heroLevels")]
        [XmlArrayItem("heroLevel")]
        public HeroLevel[] HeroLevels { get; set; }
        
        public static SongPhraseIteration2014[] Parse(Sng2014HSL.PhraseIterationSection piSection) {
            var piter = new SongPhraseIteration2014[piSection.Count];
            for (int i = 0; i < piSection.Count; i++) {
                var pi = new SongPhraseIteration2014();
                pi.PhraseId = piSection.PhraseIterations[i].PhraseId;
                pi.Time = piSection.PhraseIterations[i].StartTime;
                pi.Variation = "";
                if (!piSection.PhraseIterations[i].Difficulty.SequenceEqual(new Int32[] { 0,0,0 }))
                    pi.HeroLevels = HeroLevel.Parse(piSection.PhraseIterations[i]);
                piter[i] = pi;
            }
            return piter;
        }
    }

    [XmlType("heroLevels")]
    public class HeroLevel {        
        [XmlAttribute("difficulty")]
        public Byte Difficulty { get; set; }

        [XmlAttribute("hero")]
        public int Hero { get; set; }

        internal static HeroLevel[] Parse(DLCPackage.Manifest.PhraseIteration phraseIteration) {
            var heroLevels = new HeroLevel[3];
            for (var i = 0; i < heroLevels.Length; i++) {
                var hero = new HeroLevel();
                hero.Hero = i + 1;
                hero.Difficulty = (byte)phraseIteration.MaxScorePerDifficulty[i];
                heroLevels[i] = hero;
            }
            return heroLevels;
        }

        internal static HeroLevel[] Parse(Sng2014HSL.PhraseIteration phraseIteration) {
            var heroLevels = new HeroLevel[3];
            for(var i = 0; i < heroLevels.Length; i++) {
                var hero = new HeroLevel();
                hero.Hero = i + 1;
                hero.Difficulty = (byte)phraseIteration.Difficulty[i];
                heroLevels[i] = hero;
            }
            return heroLevels;
        }
    }

    [XmlType("newLinkedDiffs")]
    public class SongNewLinkedDiff {
        [XmlAttribute("levelBreak")]
        public Int32 LevelBreak { get; set; }

        [XmlAttribute("ratio")]
        public string Ratio { get; set; }

        [XmlAttribute("phraseCount")]
        public Int32 PhraseCount { get; set; }

        [XmlElement("nld_phrase")]
        public List<SongNld_phrase> Nld_phrase { get; set; }

        public static SongNewLinkedDiff[] Parse(Sng2014HSL.NLinkedDifficultySection nlinkedDifficultySection) {
            var newLinkedDiff = new SongNewLinkedDiff[nlinkedDifficultySection.Count];
            for (int i = 0; i < nlinkedDifficultySection.Count; i++) {
                var nld = new SongNewLinkedDiff();
                nld.LevelBreak = nlinkedDifficultySection.NLinkedDifficulties[i].LevelBreak;
                nld.PhraseCount = nlinkedDifficultySection.NLinkedDifficulties[i].PhraseCount;
                nld.Nld_phrase = SongNld_phrase.Parse(nlinkedDifficultySection.NLinkedDifficulties[i].NLD_Phrase);
                nld.Ratio = ""; //TODO: ???
                newLinkedDiff[i] = nld;
            }
            return newLinkedDiff;
        }
    }

    [XmlType("nld_phrase")]
    public class SongNld_phrase {
        [XmlAttribute("id")]
        public Int32 Id { get; set; }

        internal static List<SongNld_phrase> Parse(int[] nldp) {
            var songNldp = new List<SongNld_phrase>();
            foreach (var n in nldp)
                songNldp.Add(new SongNld_phrase() { Id = n });
            return songNldp;
        }
    }

    public class SongChordTemplate2014 
    {
        [XmlAttribute("displayName")]
        public string DisplayName { get; set; }

        [XmlAttribute("chordName")]
        public string ChordName { get; set; }

        [XmlAttribute("fret0")]
        public sbyte Fret0 { get; set; }

        [XmlAttribute("fret1")]
        public sbyte Fret1 { get; set; }

        [XmlAttribute("fret2")]
        public sbyte Fret2 { get; set; }

        [XmlAttribute("fret3")]
        public sbyte Fret3 { get; set; }

        [XmlAttribute("fret4")]
        public sbyte Fret4 { get; set; }

        [XmlAttribute("fret5")]
        public sbyte Fret5 { get; set; }

        [XmlAttribute("finger0")]
        public sbyte Finger0 { get; set; }

        [XmlAttribute("finger1")]
        public sbyte Finger1 { get; set; }

        [XmlAttribute("finger2")]
        public sbyte Finger2 { get; set; }

        [XmlAttribute("finger3")]
        public sbyte Finger3 { get; set; }

        [XmlAttribute("finger4")]
        public sbyte Finger4 { get; set; }

        [XmlAttribute("finger5")]
        public sbyte Finger5 { get; set; }

        public int? ChordId { get; set; }

        internal static SongChordTemplate2014[] Parse(List<DLCPackage.Manifest.ChordTemplate> cteamplateList) {
            var chordTemplates = new SongChordTemplate2014[cteamplateList.Count];
            for (int i = 0; i < cteamplateList.Count; i++) {
                var sct2014 = new SongChordTemplate2014();
                sct2014.ChordName = sct2014.DisplayName = cteamplateList[i].ChordName;
                sct2014.Finger0 = (sbyte)cteamplateList[i].Fingers[0];
                sct2014.Finger1 = (sbyte)cteamplateList[i].Fingers[1];
                sct2014.Finger2 = (sbyte)cteamplateList[i].Fingers[2];
                sct2014.Finger3 = (sbyte)cteamplateList[i].Fingers[3];
                sct2014.Finger4 = (sbyte)cteamplateList[i].Fingers[4];
                sct2014.Finger5 = (sbyte)cteamplateList[i].Fingers[5];
                sct2014.Fret0 = (sbyte)cteamplateList[i].Frets[0];
                sct2014.Fret1 = (sbyte)cteamplateList[i].Frets[1];
                sct2014.Fret2 = (sbyte)cteamplateList[i].Frets[2];
                sct2014.Fret3 = (sbyte)cteamplateList[i].Frets[3];
                sct2014.Fret4 = (sbyte)cteamplateList[i].Frets[4];
                sct2014.Fret5 = (sbyte)cteamplateList[i].Frets[5];
                sct2014.ChordId = cteamplateList[i].ChordId;
                chordTemplates[i] = sct2014;
            }
            return chordTemplates;
        }

        internal static SongChordTemplate2014[] Parse(Sng2014HSL.ChordSection chordSection) {
            var chordTemplates = new SongChordTemplate2014[chordSection.Count];
            for (int i = 0; i < chordSection.Count; i++) {
                var sct2014 = new SongChordTemplate2014();
                sct2014.ChordName = sct2014.DisplayName = chordSection.Chords[i].Name.ToNullTerminatedAscii();
                sct2014.Finger0 = (sbyte)chordSection.Chords[i].Fingers[0];
                sct2014.Finger1 = (sbyte)chordSection.Chords[i].Fingers[1];
                sct2014.Finger2 = (sbyte)chordSection.Chords[i].Fingers[2];
                sct2014.Finger3 = (sbyte)chordSection.Chords[i].Fingers[3];
                sct2014.Finger4 = (sbyte)chordSection.Chords[i].Fingers[4];
                sct2014.Finger5 = (sbyte)chordSection.Chords[i].Fingers[5];
                sct2014.Fret0 = (sbyte)chordSection.Chords[i].Frets[0];
                sct2014.Fret1 = (sbyte)chordSection.Chords[i].Frets[1];
                sct2014.Fret2 = (sbyte)chordSection.Chords[i].Frets[2];
                sct2014.Fret3 = (sbyte)chordSection.Chords[i].Frets[3];
                sct2014.Fret4 = (sbyte)chordSection.Chords[i].Frets[4];
                sct2014.Fret5 = (sbyte)chordSection.Chords[i].Frets[5];
                sct2014.ChordId = null;
                
                // Parse chord mask
                var mask = chordSection.Chords[i].Mask;
                if ((mask & Sng2014HSL.Sng2014FileWriter.CHORD_MASK_ARPEGGIO) != 0) {
                    mask &= ~Sng2014HSL.Sng2014FileWriter.CHORD_MASK_ARPEGGIO;
                    sct2014.DisplayName += "-arp";
                } else if ((mask & Sng2014HSL.Sng2014FileWriter.CHORD_MASK_NOP) != 0) {
                    mask &= ~Sng2014HSL.Sng2014FileWriter.CHORD_MASK_NOP;
                    sct2014.DisplayName += "-nop";
                }

                chordTemplates[i] = sct2014;
            }
            return chordTemplates;
        }

        internal static SongChordTemplate2014[] AddChordIds(SongChordTemplate2014[] chordTemplates, List<DLCPackage.Manifest.ChordTemplate> ctemplateList)
        {
            for (int i = 0; i < ctemplateList.Count; i++)
            {
                var ct = ctemplateList[i];
                var matchingChord = chordTemplates.First(sct =>
                    sct.Fret0 == ct.Frets[0] &&
                    sct.Fret1 == ct.Frets[1] &&
                    sct.Fret2 == ct.Frets[2] &&
                    sct.Fret3 == ct.Frets[3] &&
                    sct.Fret4 == ct.Frets[4] &&
                    sct.Fret5 == ct.Frets[5]);
                matchingChord.ChordId = ct.ChordId;
            }
            return chordTemplates;
        }
    }

    public class SongLevel2014 {
        [XmlAttribute("difficulty")]
        public Int32 Difficulty { get; set; }

        [XmlArray("notes")]
        [XmlArrayItem("note")]
        public SongNote2014[] Notes { get; set; }

        [XmlArray("chords")]
        [XmlArrayItem("chord")]
        public SongChord2014[] Chords { get; set; }

        [XmlArray("anchors")]
        [XmlArrayItem("anchor")]
        public SongAnchor2014[] Anchors { get; set; }

        [XmlArray("handShapes")]
        [XmlArrayItem("handShape")]
        public SongHandShape[] HandShapes { get; set; }

        internal static SongLevel2014[] Parse(Sng2014HSL.Sng sngData) {
            var levels = new SongLevel2014[sngData.Arrangements.Count];
            for (var i = 0; i < sngData.Arrangements.Count; i++) {
                var level = new SongLevel2014();
                level.Difficulty = sngData.Arrangements.Arrangements[i].Difficulty;
                level.Notes = SongNote2014.Parse(sngData.Arrangements.Arrangements[i].Notes);
                level.Chords = SongChord2014.Parse(sngData, sngData.Arrangements.Arrangements[i].Notes);
                level.Anchors = SongAnchor2014.Parse(sngData.Arrangements.Arrangements[i].Anchors);
                level.HandShapes = SongHandShape.Parse(sngData.Arrangements.Arrangements[i]);
                levels[i] = level;
            }
            return levels;
        }
    }

    public class SongNote2014
    {
        [XmlAttribute("time")]
        public Single Time { get; set; }

        [XmlAttribute("linkNext")]
        public Int32 LinkNext { get; set; }

        [XmlAttribute("accent")]
        public Int32 Accent { get; set; }

        [XmlAttribute("bend")]
        public Byte Bend { get; set; }

        [XmlAttribute("fret")]
        public SByte Fret { get; set; }

        [XmlAttribute("hammerOn")]
        public Byte HammerOn { get; set; }

        [XmlAttribute("harmonic")]
        public Byte Harmonic { get; set; }

        [XmlAttribute("hopo")]
        public Byte Hopo { get; set; }

        [XmlAttribute("ignore")]
        public Byte Ignore { get; set; }

        [XmlAttribute("leftHand")]
        public SByte LeftHand { get; set; }

        [XmlAttribute("mute")]
        public Int32 Mute { get; set; }

        [XmlAttribute("palmMute")]
        public Byte PalmMute { get; set; }

        [XmlAttribute("pluck")]
        public sbyte Pluck { get; set; }

        [XmlAttribute("pullOff")]
        public Byte PullOff { get; set; }

        [XmlAttribute("slap")]
        public sbyte Slap { get; set; }

        [XmlAttribute("slideTo")]
        public sbyte SlideTo { get; set; }

        [XmlAttribute("string")]
        public Byte String { get; set; }

        [XmlAttribute("sustain")]
        public Single Sustain { get; set; }

        [XmlAttribute("tremolo")]
        public Byte Tremolo { get; set; }

        [XmlAttribute("harmonicPinch")]
        public Int32 HarmonicPinch { get; set; }

        [XmlAttribute("pickDirection")]
        public Int32 PickDirection { get; set; }

        [XmlAttribute("rightHand")]
        public Int32 RightHand { get; set; }

        [XmlAttribute("slideUnpitchTo")]
        public SByte SlideUnpitchTo { get; set; }

        [XmlAttribute("tap")]
        public Byte Tap { get; set; }

        [XmlAttribute("vibrato")]
        public Int16 Vibrato { get; set; }

        [XmlArray("bendValues")]
        [XmlArrayItem("bendValue")]
        public BendValue[] BendValues { get; set; }

        internal static SongNote2014[] Parse(Sng2014HSL.NotesSection notesSection) {
            var notes = new List<SongNote2014>();
            
            for (var i = 0; i < notesSection.Count; i++) {
                if (notesSection.Notes[i].ChordId != -1)
                    continue; //Skip chord notes (get only single notes)

                var note = new SongNote2014();

                // BASIC INFO
                note.Time = notesSection.Notes[i].Time;
                note.Fret = (sbyte)notesSection.Notes[i].FretId;
                note.String = notesSection.Notes[i].StringIndex;

                // TECHNIQUES
                note.PickDirection = notesSection.Notes[i].PickDirection;
                note.parseNoteMask(notesSection.Notes[i].NoteMask); //NOTE MASK need to be setup previous get property values
                // Techniques with own properties
                if (notesSection.Notes[i].LeftHand != 255) note.LeftHand = (sbyte)notesSection.Notes[i].LeftHand;
                if (notesSection.Notes[i].SlideTo != 255) note.SlideTo = (sbyte)notesSection.Notes[i].SlideTo;
                if (notesSection.Notes[i].SlideUnpitchTo != 255) note.SlideUnpitchTo = (sbyte)notesSection.Notes[i].SlideUnpitchTo;
                if (notesSection.Notes[i].Tap != 255) note.Tap = notesSection.Notes[i].Tap;
                if (notesSection.Notes[i].Slap != 255) note.Slap = (sbyte)notesSection.Notes[i].Slap;
                if (notesSection.Notes[i].Pluck != 255) note.Pluck = (sbyte)notesSection.Notes[i].Pluck;
                if (notesSection.Notes[i].Vibrato != 0) note.Vibrato = notesSection.Notes[i].Vibrato;
                if (notesSection.Notes[i].Sustain != 0) note.Sustain = notesSection.Notes[i].Sustain;
                if (notesSection.Notes[i].MaxBend != 0) note.Bend = (byte)notesSection.Notes[i].MaxBend;
                note.BendValues = BendValue.Parse(notesSection.Notes[i].BendData.BendData);

                notes.Add(note);
            }

            return notes.ToArray();
        }

        internal void parseNoteMask(uint p) {
            //Setup default non 0 values
            var notSetup = unchecked((sbyte)-1);

            this.RightHand = notSetup;
            this.LeftHand = notSetup;
            this.SlideTo = notSetup;
            this.SlideUnpitchTo = notSetup;
            this.Tap = (byte)0;
            this.Slap = notSetup;
            this.Pluck = notSetup;            
            
            // Remove flags from markers (complex techniques will be setup later)
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_SINGLE) != 0)
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_SINGLE;
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_OPEN) != 0)
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_OPEN;
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_LEFTHAND) != 0)
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_LEFTHAND;
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_SLIDE) != 0)
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_SLIDE;
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_SUSTAIN) != 0)
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_SUSTAIN;
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_SLIDEUNPITCHEDTO) != 0)
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_SLIDEUNPITCHEDTO;
            
            // Setup boolean techniques and remove flag (can be override later)
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_PARENT) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_PARENT;
                this.LinkNext = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_BEND) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_BEND;
                this.Bend = 1; //Will be setup later
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_PLUCK) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_PLUCK;
                this.Pluck = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_SLAP) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_SLAP;
                this.Slap = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_TAP) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_TAP;
                this.Tap = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_VIBRATO) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_VIBRATO;
                this.Vibrato = 1;
            }            
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_ACCENT) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_ACCENT;
                this.Accent = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_HAMMERON) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_HAMMERON;
                this.HammerOn = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_PULLOFF) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_PULLOFF;
                this.PullOff = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_HARMONIC) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_HARMONIC;
                this.Harmonic = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_PALMMUTE) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_PALMMUTE;
                this.PalmMute = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_TREMOLO) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_TREMOLO;
                this.Tremolo = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_PINCHHARMONIC) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_PINCHHARMONIC;
                this.HarmonicPinch = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_RIGHTHAND) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_RIGHTHAND;
                this.RightHand = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_IGNORE) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_IGNORE;
                this.Ignore = 1;
            }
        }
    }

    [XmlType("bendValues")]
    public class BendValue {
        [XmlAttribute("time")]
        public float Time { get; set; }

        [XmlAttribute("step")]
        public float Step { get; set; }

        [XmlAttribute("unk5")]
        public Byte Unk5 { get; set; }

        internal static BendValue[] Parse(Sng2014HSL.BendData32[] bendData) {
            var bendValues = new List<BendValue>();

            for (var i = 0; i < bendData.Length; i++) {
                if (bendData[i].Time > 0 && bendData[i].Step > 0) {
                    var bend = new BendValue();
                    bend.Time = bendData[i].Time;
                    bend.Step = bendData[i].Step;
                    bend.Unk5 = bendData[i].Unk5;
                    bendValues.Add(bend);
                }
            }

            return (bendValues.Count > 0) ? bendValues.ToArray() : null;
        }
    }

    public class SongChord2014 {
        [XmlAttribute("time")]
        public Single Time { get; set; }

        [XmlAttribute("linkNext")]
        public Int32 LinkNext { get; set; }

        [XmlAttribute("accent")]
        public Int32 Accent { get; set; }

        [XmlAttribute("chordId")]
        public Int32 ChordId { get; set; }

        [XmlAttribute("fretHandMute")]
        public Int32 FretHandMute { get; set; }

        [XmlAttribute("highDensity")]
        public Byte HighDensity { get; set; }

        [XmlAttribute("ignore")]
        public Byte Ignore { get; set; }

        [XmlAttribute("palmMute")]
        public Int32 PalmMute { get; set; }

        [XmlAttribute("hopo")]
        public Int32 Hopo { get; set; }

        [XmlAttribute("strum")]
        public string Strum { get; set; }

        [XmlElement("chordNote")]
        public SongNote2014[] ChordNotes { get; set; }

        internal static SongChord2014[] Parse(Sng2014HSL.Sng sngData, Sng2014HSL.NotesSection notesSection) {
            var chords = new List<SongChord2014>();
            
            for (var i = 0; i < notesSection.Count; i++) {
                if (notesSection.Notes[i].ChordId == -1)
                    continue; //Skip single notes (get only chord notes)

                var chord = new SongChord2014();
                chord.ChordId = notesSection.Notes[i].ChordId;
                chord.Time = notesSection.Notes[i].Time;
                
                // TECHNIQUES
                chord.parseChordMask(notesSection.Notes[i], notesSection.Notes[i].NoteMask);

                // CHORD NOTES (WITHOUT TECHNIQUES) + NOT HIGH DENSITY
                if (chord.HighDensity != 1) {
                    chord.ParseChordNotes(sngData.Chords.Chords[chord.ChordId]);
                }

                // CHORD NOTES (WITH TECHNIQUES)
                var cnId = notesSection.Notes[i].ChordNotesId;
                if (cnId != -1) {
                    if (sngData.ChordNotes.ChordNotes.Length > cnId)
                        chord.ParseChordNotes(sngData.Chords.Chords[chord.ChordId], sngData.ChordNotes.ChordNotes[cnId]);
                }
                
                chords.Add(chord);
            }

            return chords.ToArray();
        }

        private void ParseChordNotes(Sng2014HSL.Chord template, Sng2014HSL.ChordNotes chordNotes = null) {
            var notes = new List<SongNote2014>();
            var notSetup = unchecked((sbyte)-1);

            for (var i = 0; i < 6; i++) {
                if ((chordNotes != null && chordNotes.NoteMask[i] != 0) || //notes with techniques
                    (chordNotes == null && template.Frets[i] != 255)) { // Notes without techniques

                    var cnote = new SongNote2014();

                    // SETUP DEFAULT VALUES
                    cnote.RightHand = notSetup;
                    cnote.LeftHand = notSetup;
                    cnote.SlideTo = notSetup;
                    cnote.SlideUnpitchTo = notSetup;
                    cnote.Tap = (byte)0;
                    cnote.Slap = notSetup;
                    cnote.Pluck = notSetup;

                    if ((chordNotes != null && chordNotes.NoteMask[i] != 0)) {
                        // SETUP FROM OWN PROPERTIES
                        cnote.parseNoteMask(chordNotes.NoteMask[i]);
                        cnote.SlideTo = (sbyte)chordNotes.SlideTo[i];
                        cnote.SlideUnpitchTo = (sbyte)chordNotes.SlideUnpitchTo[i];
                        cnote.Vibrato = chordNotes.Vibrato[i];
                        cnote.BendValues = BendValue.Parse(chordNotes.BendData[i].BendData32);
                        //Fix bend status from step in bendvalues
                        if (cnote.BendValues != null && cnote.BendValues.Length > 0)
                            foreach (var bend in cnote.BendValues)
                                if (cnote.Bend < bend.Step)
                                    cnote.Bend = (byte)Math.Round(bend.Step);
                    }
                    
                    // BASIC INFO
                    cnote.Time = this.Time;
                    cnote.Fret = (sbyte)template.Frets[i];
                    cnote.LeftHand = (sbyte)template.Fingers[i];
                    cnote.String = (byte)i;

                    notes.Add(cnote);
                }
            }

            this.ChordNotes = notes.ToArray();
        }

        private void parseChordMask(Sng2014HSL.Notes notes, uint p) {
            // Remove flags from know techniques
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_CHORD) != 0)
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_CHORD;
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_CHORDNOTES) != 0)
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_CHORDNOTES;
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_SUSTAIN) != 0)
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_SUSTAIN;
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_DOUBLESTOP) != 0)
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_DOUBLESTOP;
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_ARPEGGIO) != 0)
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_ARPEGGIO;

            this.Strum = "down";
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_STRUM) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_STRUM;
                this.Strum = "up"; //TODO: Wrong, need research about it later
            }

            if (p == 0)
                return;

            // Setup techniques and remove flags
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_PARENT) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_PARENT;
                this.LinkNext = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_ACCENT) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_ACCENT;
                this.Accent = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_FRETHANDMUTE) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_FRETHANDMUTE;
                this.FretHandMute = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_HIGHDENSITY) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_HIGHDENSITY;
                this.HighDensity = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_IGNORE) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_IGNORE;
                this.Ignore = 1;
            }
            if ((p & Sng2014HSL.Sng2014FileWriter.NOTE_MASK_PALMMUTE) != 0) {
                p &= ~Sng2014HSL.Sng2014FileWriter.NOTE_MASK_PALMMUTE;
                this.PalmMute = 1;
            }
        }
    }

    public class SongAnchor2014 : SongAnchor {
        [XmlAttribute("width")]
        public Single Width { get; set; }

        internal static SongAnchor2014[] Parse(Sng2014HSL.AnchorSection anchorSection) {
            var anchors = new SongAnchor2014[anchorSection.Count];
            for (var i = 0; i < anchorSection.Count; i++) {
                var anchor = new SongAnchor2014();
                anchor.Time = anchorSection.Anchors[i].StartBeatTime;
                anchor.Fret = anchorSection.Anchors[i].FretId;
                anchor.Width = anchorSection.Anchors[i].Width;
                anchors[i] = anchor;
            }
            return anchors;
        }
    }

    [XmlType("tone")]
    public class SongTone2014 {
        [XmlAttribute("time")]
        public float Time { get; set; }

        [XmlAttribute("id")]
        public Int32 Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        internal static SongTone2014[] Parse(Sng2014HSL.ToneSection toneSection, Attributes2014 attr = null) {
            var tones = new SongTone2014[toneSection.Count];
            for (var i = 0; i < toneSection.Count; i++) {
                var tone = new SongTone2014();
                tone.Id = toneSection.Tones[i].ToneId;
                tone.Time = toneSection.Tones[i].Time;

                if (attr != null) {
                    // Get tone name
                    switch (tone.Id) {
                        case 0:
                            tone.Name = attr.Tone_A;
                            break;
                        case 1:
                            tone.Name = attr.Tone_B;
                            break;
                        case 2:
                            tone.Name = attr.Tone_C;
                            break;
                        case 3:
                            tone.Name = attr.Tone_D;
                            break;
                        default:
                            tone.Name = "importedtone_" + tone.Id;
                            break;
                    }
                } else
                    tone.Name = "importedtone_" + tone.Id;
                    
                tones[i] = tone;
            }
            return tones;
        }
    }
}
