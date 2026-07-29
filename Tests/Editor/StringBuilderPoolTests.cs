using Jeomseon.Pool;
using Jeomseon.Scope;
using NUnit.Framework;
using System.Text;

namespace Jeomseon.Tests
{
    public sealed class StringBuilderPoolTests
    {
        [Test]
        public void ReleaseAndGet_ReusesClearedBuilder()
        {
            using StringBuilderPool pool = new(bufferSize: 1);
            StringBuilder first = pool.Get();
            first.Append("temporary");

            pool.Release(first);
            StringBuilder second = pool.Get();

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.Length, Is.Zero);
            pool.Release(second);
        }

        [Test]
        public void Scope_DisposeReturnsBuilderToSelectedPool()
        {
            using StringBuilderPool pool = new(bufferSize: 1);
            StringBuilder scopedBuilder;

            using (StringBuilderPoolScope scope = new(pool))
            {
                scopedBuilder = scope.Get();
                scopedBuilder.Append("pooled");
            }

            StringBuilder returned = pool.Get();
            Assert.That(returned, Is.SameAs(scopedBuilder));
            Assert.That(returned.Length, Is.Zero);
            pool.Release(returned);
        }
    }
}
