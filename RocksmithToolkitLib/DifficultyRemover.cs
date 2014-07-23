// Type: RocksmithToolkitLib.DifficultyRemover
// Assembly: RocksmithToolkitLib, Version=1.1.5.6, Culture=neutral, PublicKeyToken=null
// MVID: 5F34AA64-CF4E-4E33-BA2F-614A31811C97
// Assembly location: C:\Program Files\Dredd Foxx Rocksmith Tools\RocksmithToolkitLib.dll

using Newtonsoft.Json;
using RocksmithToolkitLib.DLCPackage;
using RocksmithToolkitLib.PSARC;
using RocksmithToolkitLib.Sng2014HSL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace RocksmithToolkitLib
{
    public class DifficultyRemover
    {
          public static bool LevelsAreAllStrictSubset(DLCPackageData package)
            {
                //foreach (var arrangement in package.Arrangements)
                //{
                //    if (arrangement.Sng2014.Vocals.Count > 0)
                //        continue;
                //    foreach (var pi in arrangement.Sng2014.Phrases.Phrases)
                //    {
                //        foreach (var l in pi.)
                //    }
                //}
              return false;
            }

        public static void RemoveDifficulty(Sng2014File sng, int overlap)
        {
            if (sng.Vocals.Count <= 0)
            {
                //if (overlap < 0)
                //     DifficultyRemover.CreateNddArrangement(sng);
                //else
                 DifficultyRemover.CreateNddArrangement(sng, overlap);
            }
        }

        private void  CreateNddArrangement(Sng2014File inp)
        {
        
            // NB sng2014.arrangements are *actually* difficulty levels
            var maxDifficulty = inp.Arrangements.Arrangements.Max(a => a.Difficulty);
            inp.Arrangements.Arrangements = 
                    inp.Arrangements.Arrangements.Where(a => a.Difficulty == maxDifficulty).ToArray();
            inp.Arrangements.Count = 1;
            foreach (var phrase in inp.Phrases.Phrases) {
                phrase.MaxDifficulty = 0;
            }
        }
        private static IEqualityComparer<Fingerprint> FINGERPRINT_COMPARER = new PropertyComparer<Fingerprint>("StartTime", "EndTime", "ChordId");
        private static IEqualityComparer<Anchor> ANCHOR_COMPARER = new PropertyComparer<Anchor>("StartBeatTime", "FretId", "Width");
        private static IEqualityComparer<AnchorExtension> ANCHOR_EXTENSION_COMPARER = new PropertyComparer<AnchorExtension>("BeatTime", "FretId");

        private static void  CreateNddArrangement(Sng2014File inp, int threshold)
        {
            var inputArrangements = inp.Arrangements.Arrangements;
            var maxDifficulty = inp.Arrangements.Arrangements.Max(a => a.Difficulty);
            inp.Arrangements.Arrangements = inp.Arrangements.Arrangements.Where(a => a.Difficulty == maxDifficulty).ToArray();
            var outputArrangement = inp.Arrangements.Arrangements[0];
            inp.Arrangements.Count = 1;
            foreach (var phrase in inp.Phrases.Phrases)
            {
                phrase.MaxDifficulty = 0;
            }
            
            outputArrangement.Notes.Notes =
                (from ia in inputArrangements
                from notes in ia.Notes.Notes
                select new {ia.Difficulty, notes} into dn
                group dn by dn.notes.Time into duplicateNotes
                            
                let hardest = duplicateNotes.Max(x => x.Difficulty)
                select duplicateNotes.First(x => x.Difficulty == hardest) into hardestNotes
                orderby hardestNotes.notes.Time
                select hardestNotes.notes
                ).ToArray();

                
                //inputArrangements.SelectMany(ia => ia.Notes.Notes.Select(notes => new {ia.Difficulty, notes}).GroupBy(ian => ian.notes.Time).Select(nn => nn.Max(n => n.Difficulty)).OrderBy(x => x.Time).ToArray();
            outputArrangement.Notes.Count = outputArrangement.Notes.Notes.Length;
            outputArrangement.Anchors.Anchors = inputArrangements.SelectMany(ia => ia.Anchors.Anchors).Distinct(ANCHOR_COMPARER).OrderBy(x => x.StartBeatTime).ToArray();
            outputArrangement.Anchors.Count = outputArrangement.Anchors.Anchors.Length;
            outputArrangement.AnchorExtensions.AnchorExtensions = inputArrangements.SelectMany(ia => ia.AnchorExtensions.AnchorExtensions).Distinct(ANCHOR_EXTENSION_COMPARER).ToArray();
            outputArrangement.AnchorExtensions.Count = outputArrangement.AnchorExtensions.AnchorExtensions.Length;
            outputArrangement.Fingerprints1.Fingerprints = inputArrangements.SelectMany(ia => ia.Fingerprints1.Fingerprints).Distinct(FINGERPRINT_COMPARER).OrderBy(x => x.StartTime).ToArray();
            outputArrangement.Fingerprints1.Count = outputArrangement.Fingerprints1.Fingerprints.Length;
            outputArrangement.Fingerprints2.Fingerprints = inputArrangements.SelectMany(ia => ia.Fingerprints2.Fingerprints).Distinct(FINGERPRINT_COMPARER).OrderBy(x => x.StartTime).ToArray();
            outputArrangement.Fingerprints2.Count = outputArrangement.Fingerprints2.Fingerprints.Length;

        }
    }


}
