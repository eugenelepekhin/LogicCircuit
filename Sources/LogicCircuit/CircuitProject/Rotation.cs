using System;

namespace LogicCircuit {
	public enum Rotation {
		Up,
		Right,
		Down,
		Left
	}

	public interface IRotatable {
		Rotation Rotation { get; set; }
	}
}
