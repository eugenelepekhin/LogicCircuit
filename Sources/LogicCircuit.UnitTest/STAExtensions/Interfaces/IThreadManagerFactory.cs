// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.TestFx.STAExtensions.Interfaces {
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	public interface IThreadManagerFactory {
		IThreadManager<TestResult[]> STAThreadManager { get; }
	}
}
