using System.Collections.Generic;
using CsharpToColouredHTML.Core.HeuristicsGeneration;
using CsharpToColouredHTML.Core.Miscs;
using CsharpToColouredHTML.Core.Nodes;
using Microsoft.CodeAnalysis.Classification;
using Xunit;

namespace Tests
{
    public class UtilsTests
    {
        [Fact]
        public void TypeWalkerTest_ValueTuple()
        {
            var nodes = new List<Node>()
            {
                new Node(ClassificationTypeNames.Punctuation, "(", " "),
                new Node(ClassificationTypeNames.Identifier, "MyClass", " "),
                new Node(ClassificationTypeNames.Identifier, "Success", " "),
                new Node(ClassificationTypeNames.Punctuation, ",", " "),
                new Node(ClassificationTypeNames.Identifier, "string", " "),
                new Node(ClassificationTypeNames.Punctuation, ")", " "),
                new Node(ClassificationTypeNames.MethodName, "Method", " ")
            };

            var generator = CreateGeneratorWithThoseNodes(nodes);
            var result = generator.TryConsumeTypeNameAhead();
            var output = generator.MapOutputToPublicType();

            Assert.True(result);
            Assert.Equal(NodeColors.Punctuation, output[0].Colour);
            Assert.Equal(NodeColors.Class, output[1].Colour);
            Assert.Equal(NodeColors.Identifier, output[2].Colour);
            Assert.Equal(NodeColors.Punctuation, output[3].Colour);
            Assert.Equal(NodeColors.Identifier, output[4].Colour);
            Assert.Equal(NodeColors.Punctuation, output[5].Colour);
        }

        [Fact]
        public void TypeWalkerTest_ValueTuple_TwoValuesWithoutNames()
        {
            var nodes = new List<Node>()
            {
                new Node(ClassificationTypeNames.Punctuation, "(", " "),
                new Node(ClassificationTypeNames.Identifier, "MyClass", " "),
                new Node(ClassificationTypeNames.Punctuation, ",", " "),
                new Node(ClassificationTypeNames.Identifier, "MyStruct", " "),
                new Node(ClassificationTypeNames.Punctuation, ")", " "),
            };

            var generator = CreateGeneratorWithThoseNodes(nodes);
            var result = generator.TryConsumeTypeNameAhead();
            var output = generator.MapOutputToPublicType();

            Assert.True(result);
            Assert.Equal(NodeColors.Punctuation, output[0].Colour);
            Assert.Equal(NodeColors.Class, output[1].Colour);
            Assert.Equal(NodeColors.Punctuation, output[2].Colour);
            Assert.Equal(NodeColors.Struct, output[3].Colour);
            Assert.Equal(NodeColors.Punctuation, output[4].Colour);
        }

        [Fact]
        public void TypeWalkerTest_FunctionWithTypeWithNamespace_NonTuple()
        {
            var nodes = new List<Node>()
            {
                new Node(ClassificationTypeNames.Identifier, "Abc", " "),
                new Node(ClassificationTypeNames.Operator, ".", " "),
                new Node(ClassificationTypeNames.Identifier, "Asd", " "),
                new Node(ClassificationTypeNames.Operator, ".", " "),
                new Node(ClassificationTypeNames.Identifier, "MyClass", " "),
                new Node(ClassificationTypeNames.MethodName, "Method", " "),
                new Node(ClassificationTypeNames.Punctuation, "(", " ")
            };

            var generator = CreateGeneratorWithThoseNodes(nodes);
            var result = generator.TryConsumeTypeNameAhead();
            var output = generator.MapOutputToPublicType();

            Assert.True(result);
            Assert.Equal(NodeColors.Namespace, output[0].Colour);
            Assert.Equal(NodeColors.Operator, output[1].Colour);
            Assert.Equal(NodeColors.Namespace, output[2].Colour);
            Assert.Equal(NodeColors.Operator, output[3].Colour);
            Assert.Equal(NodeColors.Class, output[4].Colour);
        }

        [Fact]
        public void TypeWalkerTest_FunctionWithTypeWithNamespace_Tuple()
        {
            var nodes = new List<Node>()
            {
                new Node(ClassificationTypeNames.Punctuation, "(", " "),
                new Node(ClassificationTypeNames.Identifier, "Abc", " "),
                new Node(ClassificationTypeNames.Operator, ".", " "),
                new Node(ClassificationTypeNames.Identifier, "Asd", " "),
                new Node(ClassificationTypeNames.Operator, ".", " "),
                new Node(ClassificationTypeNames.Identifier, "MyClass", " "),
                new Node(ClassificationTypeNames.Punctuation, ")", " "),
                new Node(ClassificationTypeNames.MethodName, "Method", " "),
                new Node(ClassificationTypeNames.Punctuation, "(", " ")
            };

            var generator = CreateGeneratorWithThoseNodes(nodes);
            var result = generator.TryConsumeTypeNameAhead();
            var output = generator.MapOutputToPublicType();

            Assert.True(result);
            Assert.Equal(NodeColors.Punctuation, output[0].Colour);
            Assert.Equal(NodeColors.Namespace, output[1].Colour);
            Assert.Equal(NodeColors.Operator, output[2].Colour);
            Assert.Equal(NodeColors.Namespace, output[3].Colour);
            Assert.Equal(NodeColors.Operator, output[4].Colour);
            Assert.Equal(NodeColors.Class, output[5].Colour);
            Assert.Equal(NodeColors.Punctuation, output[6].Colour);
        }

        [Fact]
        public void TypeWalkerTest_FunctionWithTypeWithNamespace_TupleTwoItems()
        {
            var nodes = new List<Node>()
            {
                new Node(ClassificationTypeNames.Punctuation, "(", " "),
                new Node(ClassificationTypeNames.Identifier, "Abc", " "),
                new Node(ClassificationTypeNames.Operator, ".", " "),
                new Node(ClassificationTypeNames.Identifier, "Asd", " "),
                new Node(ClassificationTypeNames.Operator, ".", " "),
                new Node(ClassificationTypeNames.Identifier, "MyClass", " "),
                new Node(ClassificationTypeNames.Punctuation, ",", " "),
                new Node(ClassificationTypeNames.Identifier, "Abc1", " "),
                new Node(ClassificationTypeNames.Operator, ".", " "),
                new Node(ClassificationTypeNames.Identifier, "Asd2", " "),
                new Node(ClassificationTypeNames.Operator, ".", " "),
                new Node(ClassificationTypeNames.Identifier, "MyStruct", " "),
                new Node(ClassificationTypeNames.Punctuation, ")", " "),
                new Node(ClassificationTypeNames.MethodName, "Method", " "),
                new Node(ClassificationTypeNames.Punctuation, "(", " ")
            };

            var generator = CreateGeneratorWithThoseNodes(nodes);
            var result = generator.TryConsumeTypeNameAhead();
            var output = generator.MapOutputToPublicType();

            Assert.True(result);

            Assert.Equal(NodeColors.Punctuation, output[0].Colour);
            Assert.Equal(NodeColors.Namespace, output[1].Colour);
            Assert.Equal(NodeColors.Operator, output[2].Colour);
            Assert.Equal(NodeColors.Namespace, output[3].Colour);
            Assert.Equal(NodeColors.Operator, output[4].Colour);
            Assert.Equal(NodeColors.Class, output[5].Colour);

            Assert.Equal(NodeColors.Punctuation, output[6].Colour);
            Assert.Equal(NodeColors.Namespace, output[7].Colour);
            Assert.Equal(NodeColors.Operator, output[8].Colour);
            Assert.Equal(NodeColors.Namespace, output[9].Colour);
            Assert.Equal(NodeColors.Operator, output[10].Colour);
            Assert.Equal(NodeColors.Struct, output[11].Colour);

            Assert.Equal(NodeColors.Punctuation, output[12].Colour);
        }

        [Fact]
        public void TypeWalkerTest_FunctionCall()
        {
            var nodes = new List<Node>()
            {
                new Node(ClassificationTypeNames.Punctuation, "Abc", " "),
                new Node(ClassificationTypeNames.Operator, ".", " "),
                new Node(ClassificationTypeNames.Identifier, "Asd", " "),
                new Node(ClassificationTypeNames.Operator, ".", " "),
                new Node(ClassificationTypeNames.Identifier, "MyClass", " "),
                new Node(ClassificationTypeNames.Operator, ".", " "),
                new Node(ClassificationTypeNames.MethodName, "Method", " "),
                new Node(ClassificationTypeNames.Punctuation, "(", " ")
            };

            var generator = CreateGeneratorWithThoseNodes(nodes);
            var result = generator.TryConsumeTypeNameAhead();
            var output = generator.MapOutputToPublicType();

            // This is function call, not TypeName
            Assert.False(result);
            Assert.Empty(output);
        }

        private static HeuristicsGenerator CreateGeneratorWithThoseNodes(List<Node> nodes)
        {
            var generator = new HeuristicsGenerator(new Hints());
            generator.Reset();
            generator._OriginalNodes = nodes;
            return generator;
        }
    }
}
