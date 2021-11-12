using System.Collections.Generic;
using SqlDsl.Core.Expressions;
using SqlDsl.Core.Predicates;

namespace SqlDsl.Core
{
    public abstract class UpdateQueryBase<TUpdateQuery> : IQuery, IHasWhereClause<TUpdateQuery> where TUpdateQuery : UpdateQueryBase<TUpdateQuery>
    {
        private readonly TableAliasExpression _table;
        private readonly List<AssignExpression> _setExpressions;
        private PredicateExpression _whereClause;

        protected UpdateQueryBase(Table table)
        {
            _table = new TableAliasExpression(table, false);
            _setExpressions = new List<AssignExpression>();
        }

        public virtual void Format(ISqlWriter sql)
        {
            sql.Append("UPDATE ", _table);
            sql.Append(" SET ", ", ", _setExpressions);
            sql.Append(" WHERE ", _whereClause);
        }

        public TUpdateQuery Set<T>(ColumnExpression<T> column, T value) => SetInternal(column, new ParamExpression<T>(value));

        public TUpdateQuery Set<T>(ColumnExpression<T> column, Expression<T> value) => SetInternal(column, value);

        private TUpdateQuery SetInternal<T>(ColumnExpression<T> left, Expression<T> right)
        {
            var expression = new AssignExpression(left.UnqualifiedName, right);
            _setExpressions.Add(expression);
            return Self();
        }

        public TUpdateQuery Where(PredicateExpression condition)
        {
            _whereClause = _whereClause == null
                ? condition
                : _whereClause.And(condition);
            return Self();
        }

        public TUpdateQuery WhereExists<TSelectQuery>(TSelectQuery query) where TSelectQuery : SelectQueryBase<TSelectQuery>, new()
        {
            var condition = new ExistsExpression<TSelectQuery>(query);
            return Where(condition);
        }

        public TUpdateQuery WhereNotExists<TSelectQuery>(TSelectQuery query) where TSelectQuery : SelectQueryBase<TSelectQuery>, new()
        {
            var condition = new NotExistsExpression<TSelectQuery>(query);
            return Where(condition);
        }

        protected TUpdateQuery Self() => this as TUpdateQuery;

        public override string ToString() => QueryFormatUtils.ToString(this);
    }
}