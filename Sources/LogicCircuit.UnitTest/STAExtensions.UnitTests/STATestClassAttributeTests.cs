// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace STAExtensions.UnitTests {
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Microsoft.VisualStudio.TestTools.UnitTesting.STAExtensions;

	[TestClass]
	public class STATestClassAttributeTests {
		[TestMethod]
		public void STATestClassAttributeMustHaveDefaultConstructorAndInheritTestClassAttribute() {
			var attr = new STATestClassAttribute();
			Assert.IsInstanceOfType(attr, typeof(TestClassAttribute), "STATestClassAttribute Must Inherit TestClassAttribute.");
		}


		[TestMethod]
		public void GetTestMethodAttributeMustReturnASTATestMethodAttributeIfDefaultTestAttributePassed() {
			var classAttr = new STATestClassAttribute();
			var defaultMethodAttr = new TestMethodAttribute();
			var actualMethodAttr = classAttr.GetTestMethodAttribute(defaultMethodAttr);

			Assert.IsInstanceOfType(actualMethodAttr, typeof(STATestMethodAttribute),
				"GetTestMethodAttribute must return STATestMethodattribute if passed default STATestMethodAttribute.");
		}

		[TestMethod]
		public void GetTestMethodAttributeMustReturnSTATestMethodAttributeAsIs() {
			var classAttr = new STATestClassAttribute();
			var expectedMethodAttr = new STATestMethodAttribute();
			var actualMethodAttr = classAttr.GetTestMethodAttribute(expectedMethodAttr);

			Assert.AreEqual(expectedMethodAttr, actualMethodAttr, "GetTestMethodAttribute must return STATestMethodattribute as is.");
		}
	}
}
