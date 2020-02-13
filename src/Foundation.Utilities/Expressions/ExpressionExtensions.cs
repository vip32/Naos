namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class ExpressionExtensions
    {
        /// <summary>
        /// Expands the specified expression.
        /// </summary>
        /// <typeparam name="T">The type of the delegate.</typeparam>
        /// <param name="expression">The expression to expand.</param>
        public static Expression<T> Expand<T>(this Expression<T> expression)
        {
            // source: http://www.albahari.com/nutshell/predicatebuilder.aspx & http://www.albahari.com/nutshell/linqkit.aspx
            return (Expression<T>)new ExpressionExpander().Visit(expression);
        }

        private sealed class ExpressionExpander : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, Expression> replacementParams = null;

            internal ExpressionExpander()
            {
            }

            private ExpressionExpander(Dictionary<ParameterExpression, Expression> replaceVars)
            {
                this.replacementParams = replaceVars;
            }

            protected override Expression VisitParameter(ParameterExpression expression)
            {
                if (this.replacementParams?.ContainsKey(expression) == true)
                {
                    return this.replacementParams[expression];
                }
                else
                {
                    return base.VisitParameter(expression);
                }
            }

            /// <summary>
            /// Flatten calls to Invoke so that Entity Framework can understand it. Calls to Invoke are generated
            /// by PredicateBuilder.
            /// </summary>
            /// <param name="invocationExpression"></param>
            protected override Expression VisitInvocation(InvocationExpression invocationExpression)
            {
                var target = invocationExpression.Expression;
                if (target is MemberExpression)
                {
                    target = this.TransformExpression((MemberExpression)target);
                }

                if (target is ConstantExpression)
                {
                    target = ((ConstantExpression)target).Value as Expression;
                }

                var lambda = (LambdaExpression)target;
                Dictionary<ParameterExpression, Expression> replacementParams;

                if (this.replacementParams == null)
                {
                    replacementParams = new Dictionary<ParameterExpression, Expression>();
                }
                else
                {
                    replacementParams = new Dictionary<ParameterExpression, Expression>(this.replacementParams);
                }

                try
                {
                    for (var i = 0; i < lambda.Parameters.Count; i++)
                    {
                        replacementParams.Add(lambda.Parameters[i], invocationExpression.Arguments[i]);
                    }
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidOperationException("Invoke cannot be called recursively - try using a temporary variable.", ex);
                }

                return new ExpressionExpander(replacementParams).Visit(lambda.Body);
            }

            protected override Expression VisitMethodCall(MethodCallExpression expression)
            {
                if (expression.Method.Name == "Invoke" && expression.Method.DeclaringType == typeof(Extensions))
                {
                    var target = expression.Arguments[0];
                    if (target is MemberExpression)
                    {
                        target = this.TransformExpression((MemberExpression)target);
                    }

                    if (target is ConstantExpression)
                    {
                        target = ((ConstantExpression)target).Value as Expression;
                    }

                    var lambda = (LambdaExpression)target;
                    Dictionary<ParameterExpression, Expression> replacementParams;

                    if (this.replacementParams == null)
                    {
                        replacementParams = new Dictionary<ParameterExpression, Expression>();
                    }
                    else
                    {
                        replacementParams = new Dictionary<ParameterExpression, Expression>(this.replacementParams);
                    }

                    try
                    {
                        for (var i = 0; i < lambda.Parameters.Count; i++)
                        {
                            replacementParams.Add(lambda.Parameters[i], expression.Arguments[i + 1]);
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        throw new InvalidOperationException("Invoke cannot be called recursively - try using a temporary variable.", ex);
                    }

                    return new ExpressionExpander(replacementParams).Visit(lambda.Body);
                }

                // Expand calls to an expression's Compile() method:
                if (expression.Method.Name == "Compile" && expression.Object is MemberExpression)
                {
                    var me = (MemberExpression)expression.Object;
                    var newExpr = this.TransformExpression(me);
                    if (newExpr != me)
                    {
                        return newExpr;
                    }
                }

                // Strip out any nested calls to AsExpandable():
                if (expression.Method.Name == "AsExpandable" && expression.Method.DeclaringType == typeof(Extensions))
                {
                    return expression.Arguments[0];
                }

                return base.VisitMethodCall(expression);
            }

            protected override Expression VisitMemberAccess(MemberExpression expression)
            {
                // Strip out any references to expressions captured by outer variables - LINQ to SQL can't handle these:
                if (expression.Member.DeclaringType.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase))
                {
                    return this.TransformExpression(expression);
                }

                return base.VisitMemberAccess(expression);
            }

            private Expression TransformExpression(MemberExpression input)
            {
                // Collapse captured outer variables
                if (input == null
                    || !(input.Member is FieldInfo)
                    || !input.Member.ReflectedType.IsNestedPrivate
                    || !input.Member.ReflectedType.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase)) // captured outer variable
                {
                    return input;
                }

                if (input.Expression is ConstantExpression)
                {
                    var obj = ((ConstantExpression)input.Expression).Value;
                    if (obj == null)
                    {
                        return input;
                    }

                    var t = obj.GetType();
                    if (!t.IsNestedPrivate || !t.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase))
                    {
                        return input;
                    }

                    var fi = (FieldInfo)input.Member;
                    var result = fi.GetValue(obj);
                    if (result is Expression)
                    {
                        return this.Visit((Expression)result);
                    }
                }

                return input;
            }
        }

        private abstract class ExpressionVisitor
        {
            public virtual Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return exp;
                }

                switch (exp.NodeType)
                {
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.ArrayLength:
                    case ExpressionType.Quote:
                    case ExpressionType.TypeAs:
                        return this.VisitUnary((UnaryExpression)exp);
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Coalesce:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.RightShift:
                    case ExpressionType.LeftShift:
                    case ExpressionType.ExclusiveOr:
                        return this.VisitBinary((BinaryExpression)exp);
                    case ExpressionType.TypeIs:
                        return this.VisitTypeIs((TypeBinaryExpression)exp);
                    case ExpressionType.Conditional:
                        return this.VisitConditional((ConditionalExpression)exp);
                    case ExpressionType.Constant:
                        return this.VisitConstant((ConstantExpression)exp);
                    case ExpressionType.Parameter:
                        return this.VisitParameter((ParameterExpression)exp);
                    case ExpressionType.MemberAccess:
                        return this.VisitMemberAccess((MemberExpression)exp);
                    case ExpressionType.Call:
                        return this.VisitMethodCall((MethodCallExpression)exp);
                    case ExpressionType.Lambda:
                        return this.VisitLambda((LambdaExpression)exp);
                    case ExpressionType.New:
                        return this.VisitNew((NewExpression)exp);
                    case ExpressionType.NewArrayInit:
                    case ExpressionType.NewArrayBounds:
                        return this.VisitNewArray((NewArrayExpression)exp);
                    case ExpressionType.Invoke:
                        return this.VisitInvocation((InvocationExpression)exp);
                    case ExpressionType.MemberInit:
                        return this.VisitMemberInit((MemberInitExpression)exp);
                    case ExpressionType.ListInit:
                        return this.VisitListInit((ListInitExpression)exp);
                    default:
                        throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
                }
            }

            protected virtual MemberBinding VisitBinding(MemberBinding binding)
            {
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        return this.VisitMemberAssignment((MemberAssignment)binding);
                    case MemberBindingType.MemberBinding:
                        return this.VisitMemberMemberBinding((MemberMemberBinding)binding);
                    case MemberBindingType.ListBinding:
                        return this.VisitMemberListBinding((MemberListBinding)binding);
                    default:
                        throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
                }
            }

            protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
            {
                var arguments = this.VisitExpressionList(initializer.Arguments);
                if (arguments != initializer.Arguments)
                {
                    return Expression.ElementInit(initializer.AddMethod, arguments);
                }

                return initializer;
            }

            protected virtual Expression VisitUnary(UnaryExpression expression)
            {
                var operand = this.Visit(expression.Operand);
                if (operand != expression.Operand)
                {
                    return Expression.MakeUnary(expression.NodeType, operand, expression.Type, expression.Method);
                }

                return expression;
            }

            protected virtual Expression VisitBinary(BinaryExpression expression)
            {
                var left = this.Visit(expression.Left);
                var right = this.Visit(expression.Right);
                var conversion = this.Visit(expression.Conversion);
                if (left != expression.Left || right != expression.Right || conversion != expression.Conversion)
                {
                    if (expression.NodeType == ExpressionType.Coalesce && expression.Conversion != null)
                    {
                        return Expression.Coalesce(left, right, conversion as LambdaExpression);
                    }
                    else
                    {
                        return Expression.MakeBinary(expression.NodeType, left, right, expression.IsLiftedToNull, expression.Method);
                    }
                }

                return expression;
            }

            protected virtual Expression VisitTypeIs(TypeBinaryExpression expression)
            {
                var expr = this.Visit(expression.Expression);
                if (expr != expression.Expression)
                {
                    return Expression.TypeIs(expr, expression.TypeOperand);
                }

                return expression;
            }

            protected virtual Expression VisitConstant(ConstantExpression expression)
            {
                return expression;
            }

            protected virtual Expression VisitConditional(ConditionalExpression expression)
            {
                var test = this.Visit(expression.Test);
                var ifTrue = this.Visit(expression.IfTrue);
                var ifFalse = this.Visit(expression.IfFalse);
                if (test != expression.Test || ifTrue != expression.IfTrue || ifFalse != expression.IfFalse)
                {
                    return Expression.Condition(test, ifTrue, ifFalse);
                }

                return expression;
            }

            protected virtual Expression VisitParameter(ParameterExpression expression)
            {
                return expression;
            }

            protected virtual Expression VisitMemberAccess(MemberExpression expression)
            {
                var exp = this.Visit(expression.Expression);
                if (exp != expression.Expression)
                {
                    return Expression.MakeMemberAccess(exp, expression.Member);
                }

                return expression;
            }

            protected virtual Expression VisitMethodCall(MethodCallExpression expression)
            {
                var obj = this.Visit(expression.Object);
                var args = this.VisitExpressionList(expression.Arguments);
                if (obj != expression.Object || args != expression.Arguments)
                {
                    return Expression.Call(obj, expression.Method, args);
                }

                return expression;
            }

            protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> expressions)
            {
                List<Expression> list = null;
                for (int i = 0, n = expressions.Count; i < n; i++)
                {
                    var expression = this.Visit(expressions[i]);
                    if (list != null)
                    {
                        list.Add(expression);
                    }
                    else if (expression != expressions[i])
                    {
                        list = new List<Expression>(n);
                        for (var j = 0; j < i; j++)
                        {
                            list.Add(expressions[j]);
                        }

                        list.Add(expression);
                    }
                }

                return list != null ? list.AsReadOnly() : expressions;
            }

            protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
            {
                var e = this.Visit(assignment.Expression);
                if (e != assignment.Expression)
                {
                    return Expression.Bind(assignment.Member, e);
                }

                return assignment;
            }

            protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
            {
                var bindings = this.VisitBindingList(binding.Bindings);
                if (bindings != binding.Bindings)
                {
                    return Expression.MemberBind(binding.Member, bindings);
                }

                return binding;
            }

            protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
            {
                var initializers = this.VisitElementInitializerList(binding.Initializers);
                if (initializers != binding.Initializers)
                {
                    return Expression.ListBind(binding.Member, initializers);
                }

                return binding;
            }

            protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
            {
                List<MemberBinding> list = null;
                for (int i = 0, n = original.Count; i < n; i++)
                {
                    var binding = this.VisitBinding(original[i]);
                    if (list != null)
                    {
                        list.Add(binding);
                    }
                    else if (binding != original[i])
                    {
                        list = new List<MemberBinding>(n);
                        for (var j = 0; j < i; j++)
                        {
                            list.Add(original[j]);
                        }

                        list.Add(binding);
                    }
                }

                return list ?? (IEnumerable<MemberBinding>)original;
            }

            protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
            {
                List<ElementInit> list = null;
                for (int i = 0, n = original.Count; i < n; i++)
                {
                    var element = this.VisitElementInitializer(original[i]);
                    if (list != null)
                    {
                        list.Add(element);
                    }
                    else if (element != original[i])
                    {
                        list = new List<ElementInit>(n);
                        for (var j = 0; j < i; j++)
                        {
                            list.Add(original[j]);
                        }

                        list.Add(element);
                    }
                }

                return list ?? (IEnumerable<ElementInit>)original;
            }

            protected virtual Expression VisitLambda(LambdaExpression lambdaExpression)
            {
                var body = this.Visit(lambdaExpression.Body);
                if (body != lambdaExpression.Body)
                {
                    return Expression.Lambda(lambdaExpression.Type, body, lambdaExpression.Parameters);
                }

                return lambdaExpression;
            }

            protected virtual NewExpression VisitNew(NewExpression newExpression)
            {
                var args = this.VisitExpressionList(newExpression.Arguments);
                if (args != newExpression.Arguments)
                {
                    if (newExpression.Members != null)
                    {
                        return Expression.New(newExpression.Constructor, args, newExpression.Members);
                    }
                    else
                    {
                        return Expression.New(newExpression.Constructor, args);
                    }
                }

                return newExpression;
            }

            protected virtual Expression VisitMemberInit(MemberInitExpression initExpression)
            {
                var expression = this.VisitNew(initExpression.NewExpression);
                var bindings = this.VisitBindingList(initExpression.Bindings);
                if (expression != initExpression.NewExpression || bindings != initExpression.Bindings)
                {
                    return Expression.MemberInit(expression, bindings);
                }

                return initExpression;
            }

            protected virtual Expression VisitListInit(ListInitExpression initExpression)
            {
                var expression = this.VisitNew(initExpression.NewExpression);
                var initializers = this.VisitElementInitializerList(initExpression.Initializers);
                if (expression != initExpression.NewExpression || initializers != initExpression.Initializers)
                {
                    return Expression.ListInit(expression, initializers);
                }

                return initExpression;
            }

            protected virtual Expression VisitNewArray(NewArrayExpression arrayExpression)
            {
                var expressions = this.VisitExpressionList(arrayExpression.Expressions);
                if (expressions != arrayExpression.Expressions)
                {
                    if (arrayExpression.NodeType == ExpressionType.NewArrayInit)
                    {
                        return Expression.NewArrayInit(arrayExpression.Type.GetElementType(), expressions);
                    }
                    else
                    {
                        return Expression.NewArrayBounds(arrayExpression.Type.GetElementType(), expressions);
                    }
                }

                return arrayExpression;
            }

            protected virtual Expression VisitInvocation(InvocationExpression invocationExpression)
            {
                var args = this.VisitExpressionList(invocationExpression.Arguments);
                var expression = this.Visit(invocationExpression.Expression);

                if (args != invocationExpression.Arguments || expression != invocationExpression.Expression)
                {
                    return Expression.Invoke(expression, args);
                }

                return invocationExpression;
            }
        }
    }
}
