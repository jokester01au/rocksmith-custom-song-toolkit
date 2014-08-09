using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RocksmithToolkitLib.Extensions;

namespace RocksmithToolkitLib.DLCPackage.AggregateGraph
{
	public abstract class ElementFile : Element
    {
        private Stream _data;
		public string File { get; set; }
        public string Name { get { return System.IO.Path.GetFileNameWithoutExtension(File); } }
        public Stream Data { get { return this._data; } set { this._data = new RecyclableStream(value); } }

		public ElementFile() {
		}
		public ElementFile(string file, Stream data) {
			File = file;
			Data = data;
		}
    }


}
