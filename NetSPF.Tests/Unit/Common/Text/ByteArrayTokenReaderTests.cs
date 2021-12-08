using System;
using System.Collections.Generic;
using System.Text;
using NetSPF.Common.Text;
using NUnit.Framework;

namespace NetSPF.Tests.Unit.Common.Text
{
    [TestFixture]
    public class ByteArrayTokenReaderTests
    {
        [Test]
        public void should_tokenize_word()
        {
            var tokens = Tokenize("ABC");

            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual(TokenType.Text, tokens[0].Type);
            Assert.AreEqual(TokenType.None, tokens[1].Type);
            Assert.AreEqual("ABC", tokens[0].Text);
        }

        [Test]
        public void should_tokenize_number()
        {
            var tokens = Tokenize("123");

            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual(TokenType.Number, tokens[0].Type);
            Assert.AreEqual(TokenType.None, tokens[1].Type);
            Assert.AreEqual("123", tokens[0].Text);
        }

        [Test]
        public void should_tokenize_alpha_numeric_with_leading_text()
        {
            var tokens = Tokenize("abc123");

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(TokenType.Text, tokens[0].Type);
            Assert.AreEqual(TokenType.Number, tokens[1].Type);
            Assert.AreEqual(TokenType.None, tokens[2].Type);
            Assert.AreEqual("abc", tokens[0].Text);
            Assert.AreEqual("123", tokens[1].Text);
        }

        [Test]
        public void should_tokenize_alpha_numeric_with_leading_number()
        {
            var tokens = Tokenize("123abc");

            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(TokenType.Number, tokens[0].Type);
            Assert.AreEqual(TokenType.Text, tokens[1].Type);
            Assert.AreEqual(TokenType.None, tokens[2].Type);
            Assert.AreEqual("123", tokens[0].Text);
            Assert.AreEqual("abc", tokens[1].Text);
        }

        [Test]
        public void should_tokenize_other()
        {
            var tokens = Tokenize("+");

            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual(TokenType.Other, tokens[0].Type);
            Assert.AreEqual(TokenType.None, tokens[1].Type);
            Assert.AreEqual("+", tokens[0].Text);
        }

        [Test]
        public void should_tokenize_space()
        {
            var tokens = Tokenize(" ");

            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual(TokenType.Space, tokens[0].Type);
            Assert.AreEqual(TokenType.None, tokens[1].Type);
        }

        [Test]
        public void should_tokenize_separator([Values("=", ":", "/")] string value)
        {
            var tokens = Tokenize(value);
            
            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual(TokenType.Separator, tokens[0].Type);
            Assert.AreEqual(TokenType.None, tokens[1].Type);
        }

        [Test]
        public void should_tokenize_sentence()
        {
            var tokens = Tokenize("The time has come");

            Assert.AreEqual(8, tokens.Count);
            Assert.AreEqual(TokenType.Text, tokens[0].Type);
            Assert.AreEqual(TokenType.Space, tokens[1].Type);
            Assert.AreEqual(TokenType.Text, tokens[2].Type);
            Assert.AreEqual(TokenType.Space, tokens[3].Type);
            Assert.AreEqual(TokenType.Text, tokens[4].Type);
            Assert.AreEqual(TokenType.Space, tokens[5].Type);
            Assert.AreEqual(TokenType.Text, tokens[6].Type);
            Assert.AreEqual(TokenType.None, tokens[7].Type);
            Assert.AreEqual("The", tokens[0].Text);
            Assert.AreEqual("time", tokens[2].Text);
            Assert.AreEqual("has", tokens[4].Text);
            Assert.AreEqual("come", tokens[6].Text);
        }

        private static IReadOnlyList<Token> Tokenize(string input)
        {
            var tokenReader = new ByteArrayTokenReader(new[] {new ArraySegment<byte>(Encoding.ASCII.GetBytes(input))});

            return tokenReader.ToList();
        }
    }
}