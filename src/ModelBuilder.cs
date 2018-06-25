using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SQLite {

	public class ModelBuilder {
		public IDictionary<Type, ITypeBuilder> TypeBuilders { get; set; } = new Dictionary<Type, ITypeBuilder> ();

		public TypeBuilder<T> Entity<T> () {
			if (!TypeBuilders.ContainsKey (typeof (T)))
				TypeBuilders.Add (typeof (T), new TypeBuilder<T> ());

			return TypeBuilders[typeof (T)] as TypeBuilder<T>;
		}

		public ITypeBuilder Entity (Type type) {
			if (TypeBuilders.ContainsKey (type))
				return TypeBuilders[type];

			return null;
		}

		public T GetAttribute<T> (PropertyInfo property)
			where T : Attribute {
			var attr = property.GetCustomAttribute<T> (true);

			if (attr != null)
				return attr;

			var typeBuilder = Entity (property.DeclaringType);

			if (typeBuilder != null)
				return Entity (property.DeclaringType).PropertyAttributes.Where (x => x.Key == property.Name)
					.SelectMany (x => x.Value).Where (x => typeof (T).GetTypeInfo ().IsAssignableFrom (x.GetType ().GetTypeInfo ()))
					.Cast<T> ().FirstOrDefault ();

			return null;
		}

		public T GetAttribute<T> (Type type)
			where T : Attribute {
			var attr = property.GetCustomAttribute<T> (true);

			if (attr != null)
				return attr;

			var typeBuilder = Entity (property.DeclaringType);

			if (typeBuilder != null)
				return Entity (property.DeclaringType).PropertyAttributes.Where (x => x.Key == property.Name)
					.SelectMany (x => x.Value).Where (x => typeof (T).GetTypeInfo ().IsAssignableFrom (x.GetType ().GetTypeInfo ()))
					.Cast<T> ().FirstOrDefault ();

			return null;
		}

		public static ModelBuilder Current { get; } = new ModelBuilder ();
	}

	public static class ModelBuilderExtensions {

		public static T GetMetaDataAttribute<T> (this PropertyInfo propertyInfo)
			where T : Attribute {
			return ModelBuilder.Current.GetAttribute<T> (propertyInfo);
		}
	}

	public interface ITypeBuilder {
		Type @Type { get; }

		IDictionary<string, List<Attribute>> PropertyAttributes { get; }
	}

	public class TypeBuilder<T> : ITypeBuilder {
		public Type Type { get; } = typeof (T);

		public IDictionary<string, List<Attribute>> PropertyAttributes { get; } = new Dictionary<string, List<Attribute>> ();

		public TypeBuilder<T> AddAttribute<TP> (Expression<Func<T, TP>> fieldSelector, params Attribute[] attributes) {
			var name = (fieldSelector.Body as MemberExpression).Member.Name;
			if (!PropertyAttributes.ContainsKey (name))
				PropertyAttributes[name] = new List<Attribute> ();

			PropertyAttributes[name].AddRange (attributes);
			return this;
		}

		public TypeBuilder<T> AddAttribute (params Attribute[] attributes) {
			var name = "";
			if (!PropertyAttributes.ContainsKey (name))
				PropertyAttributes[name] = new List<Attribute> ();

			PropertyAttributes[name].AddRange (attributes);
			return this;
		}
	}
}
