using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Xpo;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
    public static class QueryExtensions
    {
        public static List<T> GetAllOrderedBy<T, TKey>(
            this Session session,
            Func<T, TKey> keySelector)
        {
            return session.Query<T>().OrderBy(keySelector).ToList();
        }
    }
}