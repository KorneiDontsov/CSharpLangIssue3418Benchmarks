namespace CSharpLangIssue3418Benchmarks {
	using BenchmarkDotNet.Running;

	public static class Program {
		public static void Main () =>
			BenchmarkRunner.Run<BuilderAsObjectVsCopiedStructVsReferencedStruct>();
	}
}
