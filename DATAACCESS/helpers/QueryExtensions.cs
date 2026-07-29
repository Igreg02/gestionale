using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Xpo;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
    /// <summary>
    /// Extension generiche su <see cref="Session"/> per query ripetitive nei service CRUD.
    /// </summary>
    public static class QueryExtensions
    {
        /// <summary>
        /// Esegue Query&lt;T&gt;().OrderBy(selector).ToList() — boilerplate condiviso dai GetAllAsync
        /// di Company/Project/Status/Type/Employee.
        /// </summary>
        public static List<T> GetAllOrderedBy<T, TKey>(
            this Session session,
            Func<T, TKey> keySelector)
        {
            return session.Query<T>().OrderBy(keySelector).ToList();
        }
    }
}