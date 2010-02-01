// Copyright (c) 2004-2010 Avencia, Inc.
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Reflection;

namespace Avencia.Open.Geocoding
{
	/// <summary>
	/// A base class for anything with a set of properties that describes an address.
	/// </summary>
	[Serializable]
	public abstract class AddressContainer : ICloneable
	{
		/// <summary>
		/// A street address component of an address.
		/// </summary>
        public string Address { get; set; }
		/// <summary>
		/// A city component of an address.
		/// </summary>
        public string City { get; set; }
		/// <summary>
		/// A state component of an address.
		/// </summary>
        public string State { get; set; }
		/// <summary>
		/// A country component of an address.
		/// </summary>
        public string Country { get; set; }
		/// <summary>
		/// A postal code component of an address.
		/// </summary>
        public string PostalCode { get; set; }
		/// <summary>
		/// Create a new AddressContainer
		/// </summary>
		protected AddressContainer()
		{
			Address = "";
			City = "";
			State = "";
			Country = "";
			PostalCode = "";
		}
		/// <summary>
		/// Clone an existing AddressContainer
		/// </summary>
		/// <param name="original">The address container to clone</param>
		protected AddressContainer(AddressContainer original)
		{
			Address = original.Address;
			City = original.City;
			State = original.State;
			Country = original.Country;
			PostalCode = original.PostalCode;
		}
		/// <summary>
		/// Create a pretty representation of this address container.
		/// </summary>
		/// <returns>A nice text string.</returns>
		public string ToTextString()
		{
			string fullAddress = "";

			if ((Address != null) && (Address.Trim().Length > 0))
				fullAddress += Address;

			if ((City != null) && (City.Trim().Length > 0))
			{
				if ((fullAddress != null) && (fullAddress.Trim().Length > 0))
					fullAddress += ", ";
				fullAddress += City;
			}

			if ((State != null) && (State.Trim().Length > 0))
			{
				if ((fullAddress != null) && (fullAddress.Trim().Length > 0))
					fullAddress += ", ";
				fullAddress += State;
			}

			if ((PostalCode != null) && (PostalCode.Trim().Length > 0))
			{
				if ((fullAddress != null) && (fullAddress.Trim().Length > 0))
					fullAddress += " ";
				fullAddress += PostalCode;
			}

			if ((Country != null) && (Country.Trim().Length > 0))
			{
				if ((fullAddress != null) && (fullAddress.Trim().Length > 0))
					fullAddress += ", ";

				fullAddress += Country;
			}

			if ((fullAddress != null) && (fullAddress.Trim().Length > 0))
				fullAddress = fullAddress.Trim();

			return fullAddress;
		}

		#region ICloneable Members

	    ///<summary>
	    ///Creates a new object that is a copy of the current instance.
	    ///</summary>
	    ///<returns>
	    /// A new object that is a copy of this instance.
	    ///</returns>
	    public object Clone() 
		{
			ConstructorInfo ci = GetType().GetConstructor( Type.EmptyTypes );
			object obj = ci.Invoke( null );

			foreach (FieldInfo fi in obj.GetType().GetFields())
			{
				fi.SetValue(obj, fi.GetValue(this));
			}

			foreach (PropertyInfo pi in obj.GetType().GetProperties())
			{
				pi.SetValue(obj, pi.GetValue(this,null),null);
			}

			return obj;
		}

		#endregion
	}
}
