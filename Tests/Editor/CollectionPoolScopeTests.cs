using Jeomseon.Scope;
using NUnit.Framework;

namespace Jeomseon.Tests
{
    public sealed class CollectionPoolScopeTests
    {
        [Test]
        public void ArrayPoolScope_ProvidesAtLeastRequestedLength()
        {
            using ArrayPoolScope<int> scope = new(8);

            Assert.That(scope.Get().Length, Is.GreaterThanOrEqualTo(8));
        }

        [Test]
        public void ListPoolScope_DisposeClearsListBeforeReuse()
        {
            using (ListPoolScope<int> first = new())
            {
                first.Get().Add(42);
            }

            using ListPoolScope<int> second = new();
            Assert.That(second.Get(), Is.Empty);
        }

        [Test]
        public void DictionaryPoolScope_DisposeClearsDictionaryBeforeReuse()
        {
            using (DictionaryPoolScope<string, int> first = new())
            {
                first.Get().Add("value", 42);
            }

            using DictionaryPoolScope<string, int> second = new();
            Assert.That(second.Get(), Is.Empty);
        }
    }
}
