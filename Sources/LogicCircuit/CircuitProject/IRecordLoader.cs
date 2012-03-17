using System.Xml;

namespace LogicCircuit {
	public interface IRecordLoader {
		void Load(XmlReader reader);
	}
}