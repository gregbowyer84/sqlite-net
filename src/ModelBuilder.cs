using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SQLite {

	/// <summary>
	///
	/// </summary>
	public class ModelBuilder {

		/// <summary>
		///
		/// </summary>
		public IDictionary<Type, ITypeBuilder> TypeBuilders { get; set; } = new Dictionary<Type, ITypeBuilder> ();

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public TypeBuilder<T> Entity<T> () {
			if (!TypeBuilders.ContainsKey (typeof (T)))
				TypeBuilders.Add (typeof (T), new TypeBuilder<T> ());

			return TypeBuilders[typeof (T)] as TypeBuilder<T>;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public ITypeBuilder Entity (Type type) {
			if (TypeBuilders.ContainsKey (type))
				return TypeBuilders[type];

			return null;
		}

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="property"></param>
		/// <returns></returns>
		public IEnumerable<T> GetAttributes<T> (PropertyInfo property)
			where T : Attribute {
			var attrs = property.GetCustomAttributes<T> (true);

			if (attrs.Any())
				return attrs;

			var typeBuilder = Entity (property.DeclaringType);

			return typeBuilder?.PropertyAttributes.Where (x => x.Key == property.Name)
					.SelectMany (x => x.Value).Where (x => typeof (T).GetTypeInfo ().IsAssignableFrom (x.GetType ().GetTypeInfo ()))
					.Cast<T> ();
		}

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="info"></param>
		/// <returns></returns>
		public IEnumerable<T> GetAttributes<T> (MemberInfo info)
			where T : Attribute {
			var attrs = info.GetCustomAttributes<T> (true);

			if (attrs.Any())
				return attrs;

			var typeBuilder = Entity (info.DeclaringType);

			return typeBuilder?.PropertyAttributes.Where (x => x.Key == info.Name)
				.SelectMany (x => x.Value).Where (x => typeof (T).GetTypeInfo ().IsAssignableFrom (x.GetType ().GetTypeInfo ()))
				.Cast<T> ();
		}

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<T> GetAttributes<T> (Type type)
			where T : Attribute {
			var attrs = type.GetTypeInfo ().GetCustomAttributes<T> ();

			if (attrs.Any())
				return attrs;

			var typeBuilder = Entity (type);

			return typeBuilder?.TypeAttributes
				.Where (x => typeof (T).GetTypeInfo ().IsAssignableFrom (x.GetType ().GetTypeInfo ())).Cast<T> ();
		}

		/// <summary>
		///
		/// </summary>
		public static ModelBuilder Current { get; } = new ModelBuilder ();
	}

	/// <summary>
	///
	/// </summary>
	public static class ModelBuilderExtensions {

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetMetaDataAttributes<T> (this PropertyInfo propertyInfo)
			where T : Attribute {
			return ModelBuilder.Current.GetAttributes<T> (propertyInfo);
		}

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetMetaDataAttributes<T> (this Type type)
			where T : Attribute {
			return ModelBuilder.Current.GetAttributes<T> (type);
		}

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetMetaDataAttributes<T> (this TypeInfo type)
			where T : Attribute {
			return ModelBuilder.Current.GetAttributes<T> (type.AsType ());
		}

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="info"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetMetaDataAttributes<T> (this MemberInfo info)
			where T : Attribute {
			return ModelBuilder.Current.GetAttributes<T> (info);
		}
	}

	/// <summary>
	///
	/// </summary>
	public interface ITypeBuilder {

		/// <summary>
		///
		/// </summary>
		Type @Type { get; }

		/// <summary>
		///
		/// </summary>
		IDictionary<string, List<Attribute>> PropertyAttributes { get; }

		/// <summary>
		///
		/// </summary>
		List<Attribute> TypeAttributes { get; }
	}

	/// <summary>
	///
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TypeBuilder<T> : ITypeBuilder {

		/// <summary>
		///
		/// </summary>
		public Type Type { get; } = typeof (T);

		/// <summary>
		///
		/// </summary>
		public IDictionary<string, List<Attribute>> PropertyAttributes { get; } = new Dictionary<string, List<Attribute>> ();

		/// <summary>
		///
		/// </summary>
		public List<Attribute> TypeAttributes { get; } = new List<Attribute> ();

		/// <summary>
		///
		/// </summary>
		/// <typeparam name="TP"></typeparam>
		/// <param name="fieldSelector"></param>
		/// <param name="attributes"></param>
		/// <returns></returns>
		public TypeBuilder<T> AddAttribute<TP> (Expression<Func<T, TP>> fieldSelector, params Attribute[] attributes) {
			var name = (fieldSelector.Body as MemberExpression).Member.Name;
			if (!PropertyAttributes.ContainsKey (name))
				PropertyAttributes[name] = new List<Attribute> ();

			PropertyAttributes[name].AddRange (attributes);
			return this;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="attributes"></param>
		/// <returns></returns>
		public TypeBuilder<T> AddAttribute (params Attribute[] attributes) {
			var name = "";
			if (!PropertyAttributes.ContainsKey (name))
				PropertyAttributes[name] = new List<Attribute> ();

			PropertyAttributes[name].AddRange (attributes);
			return this;
		}
	}
}
