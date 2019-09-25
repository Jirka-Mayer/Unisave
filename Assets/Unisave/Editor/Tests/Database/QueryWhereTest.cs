using System.Linq;
using LightJson;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Database.Query;
using Unisave.Editor.Tests.Database.Support;

namespace Unisave.Editor.Tests.Database
{
    public class QueryWhereTest : DatabaseTestFixture
    {
        [Test]
        public void PlayerEntityWhereNameEquals()
        {
            string player = CreatePlayer();
            
            var a = new RawEntity {
                type = "PlayerEntity",
                data = new JsonObject()
                    .Add("Name", "John")
            };
            a.ownerIds.Add(player);
            Database.SaveEntity(a);
            
            var b = new RawEntity {
                type = "PlayerEntity",
                data = new JsonObject()
                    .Add("Name", "Steve")
            };
            b.ownerIds.Add(player);
            Database.SaveEntity(b);
            
            // query

            var query = new EntityQuery {
                entityType = "PlayerEntity",
            };
            query.requiredOwners.Add(new UnisavePlayer(player));
            query.whereClauses.Add(
                new BasicWhereClause(
                    JsonPath.Parse("Name"),
                    "=",
                    "John"
                )
            );
            var q = Database.QueryEntities(query).ToList();
            
            Assert.AreEqual(1, q.Count);
            Assert.AreEqual(a.id, q[0].id);
        }
        
        [Test]
        public void PlayerEntityWhereNestedValueLessThan()
        {
            string player = CreatePlayer();
            
            var a = new RawEntity {
                type = "PlayerEntity",
                data = new JsonObject()
                    .Add("Foo", new JsonObject()
                        .Add("Bar", 42)
                    )
            };
            a.ownerIds.Add(player);
            Database.SaveEntity(a);
            
            var b = new RawEntity {
                type = "PlayerEntity",
                data = new JsonObject()
                    .Add("Foo", new JsonObject()
                        .Add("Bar", 80)
                    )
            };
            b.ownerIds.Add(player);
            Database.SaveEntity(b);
            
            // query

            var query = new EntityQuery {
                entityType = "PlayerEntity",
            };
            query.requiredOwners.Add(new UnisavePlayer(player));
            query.whereClauses.Add(
                new BasicWhereClause(
                    JsonPath.Parse("Foo.Bar"),
                    "<",
                    50
                )
            );
            var q = Database.QueryEntities(query).ToList();
            
            Assert.AreEqual(1, q.Count);
            Assert.AreEqual(a.id, q[0].id);
        }
    }
}