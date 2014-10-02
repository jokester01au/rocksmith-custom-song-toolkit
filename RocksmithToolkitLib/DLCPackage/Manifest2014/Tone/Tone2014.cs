﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using System.Reflection;
using RocksmithToolkitLib.Sng;
using Newtonsoft.Json;
using System.Globalization;
using Newtonsoft.Json.Linq;
using RocksmithToolkitLib.Extensions;

namespace RocksmithToolkitLib.DLCPackage.Manifest.Tone
{
    public class Tone2014
    {
        public Gear2014 GearList { get; set; }
        public bool IsCustom { get; set; }
        [JsonProperty]
        [JsonConverter(typeof(FloatToString))]
        public float Volume { get; set; }
        public List<string> ToneDescriptors { get; set; }
        public string Key { get; set; }
        public string NameSeparator { get; set; }
        public string Name { get; set; }
        public decimal SortOrder { get; set; }

        public Tone2014()
        {   //fill with defauld amp\cab
            GearList = new Gear2014(); //{ Amp = new Pedal2014() { }, Cabinet = new Pedal2014() { } }; 
            IsCustom = true;
            Volume = -12;
            ToneDescriptors = new List<string>();
            NameSeparator = " - ";
            SortOrder = 0;
        }

        public override string ToString()
        {
            return Name;
        }

        public void Serialize(string toneSavePath) {
            var serializer = new DataContractSerializer(typeof(Tone2014));
            using (var stm = XmlWriter.Create(toneSavePath, new XmlWriterSettings() { CheckCharacters = true, Indent = true })) {
                serializer.WriteObject(stm, this);
            }
        }

        public static Tone2014 LoadFromXmlTemplateFile(string toneTemplateFilePath)
        {
            Tone2014 tone = null;
            var serializer = new DataContractSerializer(typeof(Tone2014));
            using (var stm = new XmlTextReader(toneTemplateFilePath)) {
                tone = (Tone2014)serializer.ReadObject(stm);
            }
            return tone;
        }

        #region Tone Import

        public static List<Tone2014> Import(string filePath)
        {
            List<Tone2014> tones = new List<Tone2014>();

            var toneExtension = Path.GetExtension(filePath);

            switch (toneExtension)
            {
                case ".json":
                    tones.AddRange(ReadFromManifest(filePath));
                    break;
                default:
                    var platform = Packer.GetPlatform(filePath);
                    switch (platform.platform)
                    {
                        case GamePlatform.Pc:
                        case GamePlatform.Mac:
                        case GamePlatform.XBox360:
                            return ReadFromPackage(filePath, platform);
                        case GamePlatform.PS3:
                            throw new InvalidOperationException("PS3 platform is not supported at this time :(");
                        default:
                            throw new NotSupportedException(String.Format("Unknown file extension exception '{0}'. File not supported.", toneExtension));
                    }
            }

            return tones;
        }

        #endregion

        #region Tone Manifest / Song Package

        private static List<Tone2014> ReadFromManifest(string manifestFilePath)
        {
            List<Tone2014> tones = new List<Tone2014>();

			Attributes2014 jsonManifestAttributes = Manifest2014<Attributes2014>.LoadFromFile(manifestFilePath).Entries.ToArray()[0].Value.ToArray()[0].Value;
            if (jsonManifestAttributes.ArrangementName != ArrangementName.Vocals.ToString() && jsonManifestAttributes.Tones != null)
                tones.AddRange(jsonManifestAttributes.Tones);

            return tones;
        }
        
        private static List<Tone2014> ReadFromProfile(string profilePath)
        {
        	List<Tone2014> tones = new List<Tone2014>();
        	try {                
        		using (var input = File.OpenRead(profilePath))
				using (var outMS = new MemoryStream())
                using (var br = new StreamReader(outMS))
                {
        			RijndaelEncryptor.DecryptProfile(input, outMS);
                    JToken token = JObject.Parse(br.ReadToEnd());
                    foreach (var toon in token.SelectToken("CustomTones"))
                        tones.Add(toon.ToObject<Tone2014>());
        		}
        	}
        	catch {
                throw new NotSupportedException("Unknown file format exception. File not supported.");
        	}   return tones;
        }

        private static List<Tone2014> ReadFromPackage(string packagePath, Platform platform)
        {
            if (packagePath.EndsWith("_prfldb") || packagePath.EndsWith("_profile"))
                return ReadFromProfile(packagePath);
            else
            {
                List<Tone2014> tones = new List<Tone2014>();
                string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string tmpDir = Path.Combine(appDir, Path.GetFileNameWithoutExtension(packagePath) 
                              + String.Format("_{0}", platform.platform.ToString()));

                Packer.Unpack(packagePath, appDir);
                string[] toneManifestFiles = Directory.GetFiles(tmpDir, "*.json", SearchOption.AllDirectories);

                foreach (var file in toneManifestFiles)
                    tones.AddRange(ReadFromManifest(file));

                DirectoryExtension.SafeDelete(tmpDir);

                return tones;
            }
        }

        #endregion
    }

    public class FloatToString: JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(float);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return float.Parse(serializer.Deserialize<string>(reader).Replace(',', '.'), CultureInfo.InvariantCulture);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((float)value).ToString("G", new CultureInfo("en-US")));
        }
    }
}
