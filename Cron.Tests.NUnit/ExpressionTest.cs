using System;
using System.Collections.Generic;
using Cron;
using NUnit.Framework;

namespace Cron.Tests.NUnit
{
    [TestFixture]
    public class ExpressionTest
    {
        static private void AssertGetNextValidResult(string expressionString, DateTime from, DateTime expected)
        {
            Expression expression;
            Expression.TryParse(expressionString, out expression);
            DateTime next = expression.GetNextValid(from);
            Assert.AreEqual(expected, next);
        }
         
        [Test]
        public void GetNextValid_Works()
        {
            AssertGetNextValidResult("0 0 2/3 * ?", new DateTime(2009, 9, 9), new DateTime(2009, 9, 11, 0, 0, 0));
            AssertGetNextValidResult("0 0 5/7 * ?", new DateTime(2009, 9, 5), new DateTime(2009, 9, 12, 0, 0, 0));
            AssertGetNextValidResult("0 0 5/7 * ?", new DateTime(2009, 9, 6), new DateTime(2009, 9, 12, 0, 0, 0));
            AssertGetNextValidResult("0 0 5/7 * ?", new DateTime(2009, 9, 7), new DateTime(2009, 9, 12, 0, 0, 0));
            AssertGetNextValidResult("0 0 5/7 * ?", new DateTime(2009, 9, 8), new DateTime(2009, 9, 12, 0, 0, 0));
            AssertGetNextValidResult("0 0 5/7 * ?", new DateTime(2009, 9, 9), new DateTime(2009, 9, 12, 0, 0, 0));
            AssertGetNextValidResult("0 0 5/7 * ?", new DateTime(2009, 9, 10), new DateTime(2009, 9, 12, 0, 0, 0));
            AssertGetNextValidResult("0 0 5/7 * ?", new DateTime(2009, 9, 11), new DateTime(2009, 9, 12, 0, 0, 0));
            AssertGetNextValidResult("0 0 5/7 * ?", new DateTime(2009, 9, 27), new DateTime(2009, 10, 5, 0, 0, 0));
            AssertGetNextValidResult("* * * * *", new DateTime(2008, 12, 25), new DateTime(2008, 12, 25, 0, 1, 0));
            AssertGetNextValidResult("* * * * ?", new DateTime(2008, 10, 25), new DateTime(2008, 10, 25, 0, 1, 0));
            AssertGetNextValidResult("45 3,4,5 5-24 11 ?",
                                     new DateTime(2008, 10, 25),
                                     new DateTime(2008, 11, 5, 3, 45, 0));
            AssertGetNextValidResult("15,45 3-4 * 11 *", new DateTime(2008, 10, 27), new DateTime(2008, 11, 1, 3, 15, 0));
            AssertGetNextValidResult("* * * 12 *", new DateTime(2008, 10, 25), new DateTime(2008, 12, 1, 0, 0, 0));
            AssertGetNextValidResult("* * * 12 ?", new DateTime(2008, 10, 25), new DateTime(2008, 12, 1, 0, 0, 0));
            AssertGetNextValidResult("15,45 3-4 ? * *",
                                     new DateTime(2008, 10, 25, 23, 59, 0),
                                     new DateTime(2008, 10, 26, 3, 15, 0));
            AssertGetNextValidResult("* * ? * THU", new DateTime(2008, 10, 31), new DateTime(2008, 11, 6, 0, 0, 0));
            AssertGetNextValidResult("* * 5-24 * ?", new DateTime(2008, 12, 25), new DateTime(2009, 1, 5, 0, 0, 0));
            AssertGetNextValidResult("* * 5-29 1-11 ?", new DateTime(2008, 12, 25), new DateTime(2009, 1, 5, 0, 0, 0));
            AssertGetNextValidResult("* * 5/7 1-11 ?", new DateTime(2008, 12, 25), new DateTime(2009, 1, 5, 0, 0, 0));
            AssertGetNextValidResult("1/3 */5 5/7 1-11 ?", new DateTime(2008, 12, 25), new DateTime(2009, 1, 5, 0, 1, 0));
        }

        [Test]
        public void GetNextValid_WorksForComplexDayOfWeekScenarios()
        {
            AssertGetNextValidResult("15,45 3,4 ? 11 MON-TUE", new DateTime(2008, 10, 27), new DateTime(2008, 11, 3, 3, 15, 0));
            AssertGetNextValidResult("30 4 ? * MON-FRI", new DateTime(2008, 11, 21, 4, 29, 0), new DateTime(2008, 11, 21, 4, 30, 0));
            AssertGetNextValidResult("30 4 ? * MON-FRI", new DateTime(2008, 11, 21, 4, 31, 0), new DateTime(2008, 11, 24, 4, 30, 0));
            AssertGetNextValidResult("30 4 ? * MON-FRI", new DateTime(2008, 11, 20, 4, 29, 0), new DateTime(2008, 11, 20, 4, 30, 0));
            AssertGetNextValidResult("30 4 ? * MON-FRI", new DateTime(2008, 11, 20, 4, 31, 0), new DateTime(2008, 11, 21, 4, 30, 0));
            AssertGetNextValidResult("30 4 ? * MON-FRI", new DateTime(2008, 11, 22, 4, 29, 0), new DateTime(2008, 11, 24, 4, 30, 0));
            AssertGetNextValidResult("30 4 ? * MON-FRI", new DateTime(2008, 11, 22, 4, 31, 0), new DateTime(2008, 11, 24, 4, 30, 0));
            AssertGetNextValidResult("30 4 ? * MON-FRI", new DateTime(2008, 12, 8, 4, 31, 0), new DateTime(2008, 12, 9, 4, 30, 0));
            AssertGetNextValidResult("30 4 ? * MON-FRI", new DateTime(2008, 12, 1, 4, 31, 0), new DateTime(2008, 12, 2, 4, 30, 0));
            AssertGetNextValidResult("30 4 ? * MON-FRI", new DateTime(2008, 11, 28, 4, 31, 0), new DateTime(2008, 12, 1, 4, 30, 0));
            AssertGetNextValidResult("30 4 ? * MON-FRI", new DateTime(2008, 12, 31, 4, 31, 0), new DateTime(2009, 1, 1, 4, 30, 0));
            AssertGetNextValidResult("30 4 ? * MON-FRI", new DateTime(2008, 12, 31, 4, 31, 0), new DateTime(2009, 1, 1, 4, 30, 0));
            AssertGetNextValidResult("30 4 ? * MON-FRI", new DateTime(2010, 12, 31, 4, 31, 0), new DateTime(2011, 1, 3, 4, 30, 0));
        }

        [Test]
        public void IsMatch_Works()
        {
            Expression expression;
            Expression.TryParse("45 3,4,5 5-24 11 ?", out expression);
            Assert.True(expression.IsMatch(new DateTime(2008, 11, 5, 3, 45, 0)));
            Assert.False(expression.IsMatch(new DateTime(2008, 10, 25, 0, 0, 0)));
        }

        [Test]
        public void StaticGetNextValid_Works()
        {
            Expression expression1, expression2, expression3;
            Expression.TryParse("15,45 3-4 * 11 *", out expression1);
            Expression.TryParse("* * 5-29 11 ?", out expression2);
            Expression.TryParse("1/3 */5 5/7 11 ?", out expression3);
            var expressions = new List<Expression>();
            expressions.Add(expression1);
            expressions.Add(expression2);
            expressions.Add(expression3);
            Assert.AreEqual(new DateTime(2008, 11, 1, 3, 15, 0),
                            Expression.GetNextValid(new DateTime(2008, 10, 27), expressions));
        }

        [Test]
        public void StaticIsMatch_Works()
        {
            Expression expression;
            Expression.TryParse("45 3,4,5 5-24 11 ?", out expression);
            var expressions = new List<Expression>();
            expressions.Add(expression);
            Assert.True(Expression.IsMatch(new DateTime(2008, 11, 5, 3, 45, 0), expressions));
            Assert.False(Expression.IsMatch(new DateTime(2008, 10, 25, 0, 0, 0), expressions));
        }

        [Test]
        public void TryParse_Fails()
        {
            Expression result;
            Assert.False(Expression.TryParse("* * * * * *", out result));
            Assert.False(Expression.TryParse("0/? 1 14 * *", out result));
            Assert.False(Expression.TryParse("0/5,1 14 * * *", out result));
            Assert.False(Expression.TryParse("0/5-1 14,18 * * ?", out result));
            Assert.False(Expression.TryParse("0,5-1 14,18 * * ?", out result));
            Assert.False(Expression.TryParse("1 * 23 * WED", out result));
            Assert.False(Expression.TryParse("0,5-2/1 14,18 * * ?", out result));
            Assert.False(Expression.TryParse("* * * * * * * ?", out result));
            Assert.False(Expression.TryParse("nothing", out result));
            Assert.False(Expression.TryParse("60 * * * *", out result));
            Assert.False(Expression.TryParse("* 24 * * *", out result));
            Assert.False(Expression.TryParse("* * 32 * *", out result));
            Assert.False(Expression.TryParse("* * * 13 *", out result));
            Assert.False(Expression.TryParse("* * * * NOT", out result));
            Assert.False(Expression.TryParse(null, out result));
        }

        [Test]
        public void TryParse_Works()
        {
            Expression result;
            Assert.True(Expression.TryParse("1 * * * ?", out result));
            Assert.True(Expression.TryParse("0 12 * * ?", out result));
            Assert.True(Expression.TryParse("15 10 ? * *", out result));
            Assert.True(Expression.TryParse("15 10 * * ?", out result));
            Assert.True(Expression.TryParse("15 10 * * ?", out result));
            Assert.True(Expression.TryParse("15 10 * * ?", out result));
            Assert.True(Expression.TryParse("* 14 * * ?", out result));
            Assert.True(Expression.TryParse("0/5 14 * * ?", out result));
            Assert.True(Expression.TryParse("0/5 14,18 * * ?", out result));
            Assert.True(Expression.TryParse("0-5 14 * * ?", out result));
            Assert.True(Expression.TryParse("10,44 14 ? 3 WED", out result));
            Assert.True(Expression.TryParse("15 10 ? * MON-FRI", out result));
            Assert.True(Expression.TryParse("15 10 15 * ?", out result));

            Assert.True(Expression.TryParse("* * * * *", out result));
            Assert.IsInstanceOf(typeof (EveryOccurenceExpressionSection), result.MinuteSection);
            Assert.IsInstanceOf(typeof (EveryOccurenceExpressionSection), result.HourSection);
            Assert.IsInstanceOf(typeof (EveryOccurenceExpressionSection), result.DayOfMonthSection);
            Assert.IsInstanceOf(typeof (EveryOccurenceExpressionSection), result.MonthSection);
            Assert.IsInstanceOf(typeof (EveryOccurenceExpressionSection), result.DayOfWeekSection);
        }

        [Test]
        public void DayOfMonthSpecialCharacter_L_Works()
        {
            Expression result;
            Assert.True(Expression.TryParse("* * L * ?", out result));
            Assert.IsInstanceOf(typeof(LastDayOfMonthExpressionSection), result.DayOfMonthSection);
            AssertGetNextValidResult("* * L * ?", new DateTime(2008, 11, 30, 4, 29, 0), new DateTime(2008, 11, 30, 4, 30, 0));
            AssertGetNextValidResult("* * L * ?", new DateTime(2008, 11, 14, 4, 29, 0), new DateTime(2008, 11, 30, 0, 0, 0));
        }

        [Test]
        public void DayOfMonthSpecialCharacter_W_Works()
        {
            Expression result;
            Assert.True(Expression.TryParse("* * 15W * ?", out result));
            Assert.AreEqual(15, ((SimpleExpressionSection)result.DayOfMonthSection).Value);
            Assert.IsInstanceOf(typeof(NearestWeekdayExpressionSection), result.DayOfMonthSection);
            AssertGetNextValidResult("* * 15W * ?", new DateTime(2008, 11, 1, 0, 0, 0), new DateTime(2008, 11, 14, 0, 0, 0));
            AssertGetNextValidResult("* * 15W * ?", new DateTime(2008, 12, 1, 0, 0, 0), new DateTime(2008, 12, 15, 0, 0, 0));
            AssertGetNextValidResult("* * 15W * ?", new DateTime(2008, 6, 1, 0, 0, 0), new DateTime(2008, 6, 16, 0, 0, 0));
            AssertGetNextValidResult("* * 15W * ?", new DateTime(2008, 12, 15, 4, 29, 0), new DateTime(2008, 12, 15, 4, 30, 0));
            AssertGetNextValidResult("* * 1W * ?", new DateTime(2008, 10, 15, 4, 29, 0), new DateTime(2008, 11, 3, 0, 0, 0));
        }

        [Test]
        public void DayOfMonthSpecialCharacter_Combination_LW_Works()
        {
            Expression result;
            Assert.True(Expression.TryParse("* * LW * ?", out result));
            Assert.IsInstanceOf(typeof(LastWeekDayOfMonthExpressionSection), result.DayOfMonthSection);
            AssertGetNextValidResult("* * LW * ?", new DateTime(2008, 10, 1, 0, 0, 0), new DateTime(2008, 10, 31, 0, 0, 0));
            AssertGetNextValidResult("* * LW * ?", new DateTime(2008, 11, 30, 0, 0, 0), new DateTime(2008, 12, 31, 0, 0, 0));
        }

        [Test]
        public void DayOfWeekSpecialCharacter_L_Works_Alone()
        {
            Expression result;
            Assert.True(Expression.TryParse("* * ? * L", out result));
            Assert.NotNull(result.DayOfWeekSection as SimpleExpressionSection);
            Assert.AreEqual((int)DayOfWeek.Saturday , ((SimpleExpressionSection)result.DayOfWeekSection).Value);
        }

        [Test]
        public void DayOfWeekSpecialCharacter_Hash_Works()
        {
            Expression result;
            Assert.True(Expression.TryParse("* * ? * 7#3", out result));
            Assert.IsInstanceOf(typeof(SpecifiedWeekAndWeekDayExpressionSection), result.DayOfWeekSection);
            Assert.AreEqual((int)DayOfWeek.Saturday, ((SimpleExpressionSection)result.DayOfWeekSection).Value);
            Assert.AreEqual(3, ((SpecifiedWeekAndWeekDayExpressionSection)result.DayOfWeekSection).Week);
            AssertGetNextValidResult("* * ? * 7#3", new DateTime(2008, 11, 14), new DateTime(2008, 11, 15));
            AssertGetNextValidResult("* * ? * 7#3", new DateTime(2008, 11, 20), new DateTime(2008, 12, 20));
            AssertGetNextValidResult("* * ? * 7#3", new DateTime(2008, 12, 20, 2, 5, 0), new DateTime(2008, 12, 20, 2, 6, 0));
            AssertGetNextValidResult("* * ? * 6#5", new DateTime(2008, 11, 20), new DateTime(2009, 1, 30));
        }
    }
}