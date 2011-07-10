using System;
using System.IO;
using System.Xml.Serialization;
using Envoy.TDSM_Vault;

namespace Envoy.TDSM_WarpDrive
{
    public class WarpList : VaultObject
    {
        public SerializableDictionary<string, Warp> warps;
        private XmlSerializer serializer;

        public WarpList() : base("WarpDrive")
        {
            warps = new SerializableDictionary<string, Warp>();
            serializer = new XmlSerializer(typeof(SerializableDictionary<string, Warp>));
        }

        //
        // FROM VaultObject
        //

        public override void fromXml(string xml)
        {
            StringReader stringReader = new StringReader(xml);
            warps = (SerializableDictionary<string, Warp>)serializer.Deserialize(stringReader);
            stringReader.Close();
        }

        public override string toXml()
        {
            StringWriter stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, warps);
            String xml = stringWriter.ToString();
            stringWriter.Close();
            return xml;
        }

        //
        // CONVENIENCE
        //

        public void Add(string warpName, Warp warp)
        {
            warps.Add(warpName, warp);
        }

        public void Remove(string warpName)
        {
            warps.Remove(warpName);
        }

        public void TryGetValue(string warpName, out Warp warp)
        {
            warps.TryGetValue(warpName, out warp);
        }

        public bool ContainsKey(string key)
        {
            return warps.ContainsKey(key);
        }

    }

}