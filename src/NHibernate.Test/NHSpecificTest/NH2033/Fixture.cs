using System.Linq;
using NHibernate.Criterion;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2033
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var john = new Customer
				{
					AssignedId = 1,
					Name = "John"
				};
				var other = new Customer
				{
					AssignedId = 2,
					Name = "Other"
				};
				var johnBusiness = new CustomerAddress
				{
					Customer = john,
					Type = "Business",
					Address = "123 E West Ave.",
					City = "New York",
					OtherCustomer = other
				};

				session.Save(john);
				session.Save(other);
				session.Save(johnBusiness);
				session.Flush();
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from CustomerAddress").ExecuteUpdate();
				session.CreateQuery("delete from Customer").ExecuteUpdate();
				transaction.Commit();
			}
		}

		[Test]
		public void QueryOverJoinAliasOnKeyManyToOneShouldGenerateInnerJoin()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				Customer customerAlias = null;
				var query = session.QueryOver<CustomerAddress>()
					.Where(x => x.City == "New York")
					.JoinAlias(x => x.Customer, () => customerAlias)
					.Where(() => customerAlias.Name == "John");

				var results = query.List();

				Assert.That(results, Has.Count.EqualTo(1));
				Assert.That(results[0].Address, Is.EqualTo("123 E West Ave."));
			}
		}

		[Test]
		public void QueryOverJoinAliasOnManyToOneShouldGenerateInnerJoin()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				Customer customerAlias = null;
				var query = session.QueryOver<CustomerAddress>()
					.Where(x => x.City == "New York")
					.JoinAlias(x => x.OtherCustomer, () => customerAlias)
					.Where(() => customerAlias.Name == "Other");

				var results = query.List();

				Assert.That(results, Has.Count.EqualTo(1));
				Assert.That(results[0].Address, Is.EqualTo("123 E West Ave."));
				session.Clear();
			}
		}

		[Test]
		public void LinqJoinOnKeyManyToOneShouldGenerateInnerJoin()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var query = session.Query<CustomerAddress>()
					.Where(x => x.City == "New York")
					.Where(x => x.Customer.Name == "John");

				var results = query.ToList();

				Assert.That(results, Has.Count.EqualTo(1));
				Assert.That(results[0].Address, Is.EqualTo("123 E West Ave."));
			}
		}

		[Test]
		public void CreateCriteriaOnKeyManyToOneShouldGenerateInnerJoin()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var query = session.CreateCriteria<CustomerAddress>()
					.Add(Restrictions.Eq("City", "New York"))
					.CreateCriteria("Customer")
					.Add(Restrictions.Eq("Name", "John"));

				var results = query.List<CustomerAddress>();

				Assert.That(results, Has.Count.EqualTo(1));
				Assert.That(results[0].Address, Is.EqualTo("123 E West Ave."));
			}
		}

		[Test]
		public void HqlJoinOnKeyManyToOneShouldGenerateInnerJoin()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var query = session.CreateQuery(@"
						select a
						from
							CustomerAddress a
							join a.Customer c
						where
							a.City = :city
							and c.Name = :name")
					.SetString("city", "New York")
					.SetString("name", "John");

				var results = query.List<CustomerAddress>();

				Assert.That(results, Has.Count.EqualTo(1));
				Assert.That(results[0].Address, Is.EqualTo("123 E West Ave."));
			}
		}
	}
}