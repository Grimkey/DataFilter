namespace Search
{
    using System;
    using Xunit;

    public class DataFilterTest
    {
        private static readonly DataFilter emptyTree = new DataFilter(null, null, DataFilter.BooleanOp.None);
        private static readonly DataFilter leftHandSide = new DataFilter(new DataFilter.SearchNode("LeftColumn", ">", 5));
        private static readonly DataFilter rightHandSide = new DataFilter(new DataFilter.SearchNode("RightColumn", "like", "'%abc'"));

        private static readonly DataFilter leftOnly = new DataFilter(leftHandSide, null, DataFilter.BooleanOp.None);
        private static readonly DataFilter andTree = new DataFilter(leftHandSide, rightHandSide, DataFilter.BooleanOp.And);
        private static readonly DataFilter orTree = new DataFilter(leftHandSide, rightHandSide, DataFilter.BooleanOp.Or);

        private static readonly DataFilter complexTree = new DataFilter(andTree, andTree, DataFilter.BooleanOp.Or);

        [Fact]
        public void SearchNode_ToString_ShouldReturnInSqlFormat()
        {
            var expected = "column = 5";
            var searchnode = new DataFilter.SearchNode("column", "=", 5);
            Assert.Equal(expected, searchnode.ToString());
        }

        public void DataFilter_IsEmpty_True()
        {
            Assert.True(emptyTree.IsEmpty);
        }

        public void DataFilter_IsEmpty_False()
        {
            Assert.False(leftHandSide.IsEmpty);
        }


        public void DataFilter_IsLeaf_FalseEmptyTree()
        {
            Assert.False(emptyTree.IsLeaf);
        }

        [Fact]
        public void DataFilter_IsLeaf_True()
        {
            Assert.True(leftHandSide.IsLeaf);
        }

        [Fact]
        public void DataFilter_IsLeaf_False()
        {
            Assert.False(leftOnly.IsLeaf);
        }

        [Fact]
        public void EndNodeToSql_NoElements_EmptyString()
        {
            var expected = string.Empty;
            var actual = emptyTree.ToString();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EndNodeToSql_OneElement_ReturnsLeftHandSide()
        {
            var expected = "LeftColumn > 5";
            var sqlWhere = leftOnly.ToString();

            Assert.Equal(expected, sqlWhere);
        }

        [Fact]
        public void EndNodeToSql_TwoLevel_ReturnsLeftHandSide()
        {
            var expected = "LeftColumn > 5";
            var sqlWhere = leftHandSide.ToString();

            Assert.Equal(expected, sqlWhere);
        }

        [Fact]
        public void EndNodeToSql_AndOperation_ReturnsSqlFormat()
        {
            var expected = "(LeftColumn > 5 And RightColumn like '%abc')";
            var sqlWhere = andTree.ToString();

            Assert.Equal(expected, sqlWhere);
        }

        [Fact]
        public void EndNodeToSql_OrOperation_ReturnsSqlFormat()
        {
            var expected = "(LeftColumn > 5 Or RightColumn like '%abc')";
            var sqlWhere = orTree.ToString();

            Assert.Equal(expected, sqlWhere);
        }

        [Fact]
        public void EndNodeToSql_TwoLevelTree_BothLevelsShown()
        {
            var andClause = "(LeftColumn > 5 And RightColumn like '%abc')";
            var expected = string.Format("({0} Or {0})", andClause);

            var sqlWhere = complexTree.ToString();

            Assert.Equal(expected, sqlWhere);
        }
    }

    public class DataFilter
    {
        private const string conjuctionTemplate = "({0} {1} {2})";

        public enum BooleanOp
        {
            None,
            And,
            Or
        }

        public DataFilter(DataFilter left, DataFilter right, BooleanOp operation)
        {
            this.Left = left;
            this.Right = right;
            this.Boolean = operation;
        }

        public DataFilter(SearchNode node)
        {
            this.Node = node;
        }

        public BooleanOp Boolean { get; private set; }

        public DataFilter Left { get; private set; }

        public DataFilter Right { get; private set; }

        public SearchNode Node { get; private set; }

        public bool IsLeaf
        {
            get { return this.Node != null; }
        }

        public bool IsEmpty
        {
            get
            {
                if (this.Left == null
                    && this.Right == null
                    && this.Node == null)
                {
                    return true;
                }

                return false;
            }
        }

        public override string ToString()
        {
            if (this.IsEmpty)
            {
                return string.Empty;
            }

            if (this.IsLeaf)
            {
                return this.Node.ToString();
            }

            var left = this.Left.ToString();
            var right = this.Right?.ToString();

            if (right == null) { return left; }

            switch (this.Boolean)
            {
                case DataFilter.BooleanOp.And:
                case DataFilter.BooleanOp.Or:
                    return string.Format(conjuctionTemplate, left, this.Boolean, right);
                default:
                    var message = string.Format("The operation {0} has not been implemented within the QueryTree.", this.Boolean);
                    throw new NotImplementedException(message);
            }
        }

        public class SearchNode
        {
            public SearchNode(string key, string operation, object value)
            {
                this.Key = key;
                this.Operation = operation;
                this.Value = value;
            }

            public string Key { get; private set; }

            public string Operation { get; private set; }

            public object Value { get; private set; }

            public override string ToString()
            {
                return string.Format("{0} {1} {2}", this.Key, this.Operation, this.Value);
            }
        }
    }
}
