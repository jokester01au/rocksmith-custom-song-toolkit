using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RocksmithToolkitLib.DLCPackage.AggregateGraph
{
	public abstract class ElementFile : Element
    {
		public string File { get; set; }
        public string Name { get { return System.IO.Path.GetFileNameWithoutExtension(File); } }
		public Stream Data { get; set; }

		public ElementFile() {
		}
		public ElementFile(string file, Stream data) {
			File = file;
			Data = data;
		}
    }


}
