namespace Naos.Core.UnitTests.Common.Utility
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Common;
    using Shouldly;
    using Xunit;

    public class RetryTests
    {
        private static Func<int, TimeSpan> progressive = x => TimeSpan.FromMilliseconds(
            Convert.ToInt32(Math.Round((1 / (1 + Math.Exp(-x + 5))) * 100)) * 100);

        [Fact]
        public void WhenRetryTaskThat_does_not_fail()
        {
            Should.NotThrow(async () =>
            {
                var counter = 0;
                await Retry.On<NullReferenceException>(async () =>
                {
                    await Task.Yield();
                    counter++;
                });
                counter.ShouldBe(1);
            });

            Should.NotThrow(async () =>
            {
                var counter = 0;
                await Retry.On<NullReferenceException>(async () =>
                {
                    await Task.Yield();
                    counter++;
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());
                counter.ShouldBe(1);
            });
        }

        [Fact]
        public void WhenRetryTaskThat_fails_once()
        {
            Should.NotThrow(async () =>
            {
                var counter = 0;
                await Retry.On<NullReferenceException>(async () =>
                {
                    await Task.Yield();
                    if(counter++ == 0)
                    {
                        throw new NullReferenceException();
                    }
                });
                counter.ShouldBe(2);
            });

            Should.NotThrow(async () =>
            {
                var counter = 0;
                await Retry.On<NullReferenceException>(async () =>
                {
                    await Task.Yield();
                    if(counter++ == 0)
                    {
                        throw new NullReferenceException();
                    }
                },
                    100.Milliseconds());
                counter.ShouldBe(2);
            });
        }

        [Fact]
        public void WhenRetryTaskThat_fails_twice_but_succeeds_eventually()
        {
            Should.NotThrow(async () =>
            {
                var result = 0;
                var counter = 0;
                await Retry.On<NullReferenceException>(async () =>
                {
                    await Task.Yield();
                    if(counter++ < 2)
                    {
                        throw new NullReferenceException();
                    }

                    result = 42;
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());

                counter.ShouldBe(3);
                result.ShouldBe(42);
            });
        }

        [Fact]
        public void WhenRetryTaskThat_always_fails()
        {
            var counter = 0;

            var retryEx = Should.Throw<RetryException>(async () =>
            {
                await Retry.On<NullReferenceException>(async () =>
                {
                    await Task.Yield();
                    counter++;
                    throw new NullReferenceException();
                });
            });
            retryEx.RetryCount.ShouldBe(1);
            retryEx.Message.ShouldBe("retry failed after #1 attempts");

            counter.ShouldBe(2);

            counter = 0;

            retryEx = Should.Throw<RetryException>(async () =>
            {
                await Retry.On<NullReferenceException>(() =>
                {
                    counter++;
                    throw new NullReferenceException();
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());
            });
            retryEx.RetryCount.ShouldBe(3);
            retryEx.Message.ShouldBe("retry failed after #3 attempts");

            counter.ShouldBe(4);
        }

        [Fact]
        public void WhenRetryTaskThat_does_not_fail_on_multiple_exceptions()
        {
            Should.NotThrow(async () =>
            {
                var counter = 0;
                await Retry.OnAny<ArgumentNullException, NullReferenceException>(async () =>
                {
                    await Task.Yield();
                    counter++;
                });
                counter.ShouldBe(1);
            });

            Should.NotThrow(async () =>
            {
                var counter = 0;
                await Retry.OnAny<ArgumentNullException, NullReferenceException>(async () =>
                {
                    await Task.Yield();
                    counter++;
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());
                counter.ShouldBe(1);
            });
        }

        [Fact]
        public void WhenRetryTaskThat_fails_twice_but_succeeds_eventually_on_multiple_exceptions()
        {
            Should.NotThrow(async () =>
            {
                var result = 0;
                var counter = 0;
                await Retry.OnAny<ArgumentNullException, NullReferenceException>(async () =>
                {
                    await Task.Yield();
                    if(counter++ < 2)
                    {
                        throw new NullReferenceException();
                    }

                    result = 42;
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());

                counter.ShouldBe(3);
                result.ShouldBe(42);
            });
        }

        [Fact]
        public void WhenRetryTaskThat_always_fails_on_multiple_exceptions()
        {
            var counter = 0;

            var retryEx = Should.Throw<RetryException>(async () =>
            {
                await Retry.OnAny<ArgumentNullException, NullReferenceException>(() =>
                {
                    counter++;
                    throw new NullReferenceException();
                });
            });
            retryEx.RetryCount.ShouldBe(1);
            retryEx.Message.ShouldBe("retry failed after #1 attempts");

            counter.ShouldBe(2);

            counter = 0;

            retryEx = Should.Throw<RetryException>(async () =>
            {
                await Retry.OnAny<ArgumentNullException, NullReferenceException>(() =>
                {
                    counter++;
                    throw new NullReferenceException();
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());
            });
            retryEx.RetryCount.ShouldBe(3);
            retryEx.Message.ShouldBe("retry failed after #3 attempts");

            counter.ShouldBe(4);
        }

        [Fact]
        public void WhenRetryTaskThat_throws_aggregate_exception_one()
        {
            var counter = 0;

            var retryEx = Should.Throw<RetryException>(async () =>
            {
                await Retry.On<ArgumentNullException>(() =>
                {
                    counter++;
                    var inner = new ArgumentNullException("someArg");
                    throw new AggregateException(inner);
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());
            });

            retryEx.RetryCount.ShouldBe(3);
            retryEx.InnerException.ShouldBeOfType<AggregateException>();
            retryEx.Message.ShouldBe("retry failed after #3 attempts");

            counter.ShouldBe(4);
        }

        [Fact]
        public void WhenRetryTaskThat_throws_aggregate_exception_two()
        {
            var counter = 0;

            var retryEx = Should.Throw<RetryException>(async () =>
            {
                await Retry.On<IndexOutOfRangeException>(() =>
                {
                    counter++;
                    var inner1 = new ArgumentNullException("someArg1");
                    var inner2 = new IndexOutOfRangeException("someArg2");
                    throw new AggregateException(inner1, inner2);
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());
            });

            retryEx.RetryCount.ShouldBe(3);
            retryEx.InnerException.ShouldBeOfType<AggregateException>();
            retryEx.Message.ShouldBe("retry failed after #3 attempts");

            counter.ShouldBe(4);
        }

        [Fact]
        public void WhenRetryTaskThat_throws_aggregate_exception_and_no_expected_exception()
        {
            var counter = 0;

            Should.Throw<IndexOutOfRangeException>(async () =>
            {
                await Retry.On<ArgumentNullException>(() =>
                {
                    counter++;
                    var inner = new IndexOutOfRangeException();
                    throw new AggregateException(inner);
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());
            });

            counter.ShouldBe(1);
        }

        [Fact]
        public void WhenRetryTaskWith_a_predicate_returning_true()
        {
            var predicateCounter = 0;

            Func<Exception, bool> exceptionPredicate = e =>
            {
                e.ShouldBeOfType<ArgumentException>();
                predicateCounter++;
                return true;
            };

            var executionCounter = 0;

            var retryEx = Should.Throw<RetryException>(async () =>
            {
                await Retry.On(() =>
                {
                    executionCounter++;
                    throw new ArgumentException();
                }, exceptionPredicate);
            });

            retryEx.RetryCount.ShouldBe(1);
            retryEx.InnerException.ShouldBeOfType<ArgumentException>();
            retryEx.Message.ShouldBe("retry failed after #1 attempts");

            executionCounter.ShouldBe(2);
            predicateCounter.ShouldBe(1);
        }

        [Fact]
        public void WhenRetryTaskWith_a_predicate_returning_false()
        {
            var predicateCounter = 0;

            Func<Exception, bool> exceptionPredicate = e =>
            {
                e.ShouldBeOfType<ArgumentException>();
                predicateCounter++;
                return false;
            };

            var executionCounter = 0;

            Should.Throw<ArgumentException>(async () =>
            {
                await Retry.On(() =>
                {
                    executionCounter++;
                    throw new ArgumentException();
                }, exceptionPredicate);
            });

            executionCounter.ShouldBe(1);
            predicateCounter.ShouldBe(1);
        }

        [Fact]
        public void WhenRetryTaskWith_delay_factory()
        {
            var cts = new CancellationTokenSource();

            var predicateCounter = 0;

            Func<Exception, bool> exceptionPredicate = e =>
            {
                e.ShouldBeOfType<ArgumentException>();
                predicateCounter++;
                return true;
            };

            Func<int, TimeSpan> delayFactory = failureCount =>
            {
                if(failureCount == 4)
                {
                    cts.Cancel();
                    return 0.Seconds();
                }

                return progressive(failureCount);
            };

            var executionCounter = 0;

            var retryEx = Should.Throw<RetryException>(async () =>
            {
                await Retry.On(async () =>
                {
                    await Task.Delay(1);
                    executionCounter++;
                    throw new ArgumentException();
                }, exceptionPredicate, delayFactory, cts.Token);
            });

            retryEx.RetryCount.ShouldBe(3);
            retryEx.InnerException.ShouldBeOfType<ArgumentException>();
            retryEx.Message.ShouldBe("retry failed after #3 attempts");

            executionCounter.ShouldBe(4);
            predicateCounter.ShouldBe(4);
        }

        [Fact]
        public void WhenRetryResultTaskThat_does_not_fail()
        {
            Should.NotThrow(async () =>
            {
                var counter = 0;
                var result = await Retry.On<NullReferenceException, int>(async () =>
                {
                    await Task.Yield();
                    counter++;
                    return 42;
                });

                counter.ShouldBe(1);
                result.ShouldBe(42);
            });

            Should.NotThrow(async () =>
            {
                var counter = 0;
                var result = await Retry.On<NullReferenceException, int>(async () =>
                {
                    await Task.Yield();
                    counter++;
                    return 42;
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());

                counter.ShouldBe(1);
                result.ShouldBe(42);
            });
        }

        [Fact]
        public void WhenRetryResultTaskThat_fails_once()
        {
            Should.NotThrow(async () =>
            {
                var counter = 0;
                var result = await Retry.On<NullReferenceException, int>(async () =>
                {
                    await Task.Yield();
                    if(counter++ == 0)
                    {
                        throw new NullReferenceException();
                    }

                    return 42;
                });

                counter.ShouldBe(2);
                result.ShouldBe(42);
            });

            Should.NotThrow(async () =>
            {
                var counter = 0;
                var result = await Retry.On<NullReferenceException, int>(async () =>
                {
                    await Task.Yield();
                    if(counter++ == 0)
                    {
                        throw new NullReferenceException();
                    }

                    return 42;
                },
                    100.Milliseconds());

                counter.ShouldBe(2);
                result.ShouldBe(42);
            });
        }

        [Fact]
        public void WhenRetryResultTaskThat_fails_twice_but_succeeds_eventually()
        {
            Should.NotThrow(async () =>
            {
                var counter = 0;
                var result = await Retry.On<NullReferenceException, int>(async () =>
                {
                    await Task.Yield();
                    if(counter++ < 2)
                    {
                        throw new NullReferenceException();
                    }

                    return 42;
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());

                counter.ShouldBe(3);
                result.ShouldBe(42);
            });
        }

        [Fact]
        public void WhenRetryResultTaskThat_always_fails()
        {
            var result = -1;
            var counter = 0;
            var retryEx = Should.Throw<RetryException>(async () =>
            {
                result = await Retry.On<NullReferenceException, int>(() =>
                {
                    counter++;
                    throw new NullReferenceException();
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());
            });
            retryEx.RetryCount.ShouldBe(3);
            retryEx.Message.ShouldBe("retry failed after #3 attempts");

            counter.ShouldBe(4);
            result.ShouldBe(-1);

            result = -1;
            counter = 0;
            retryEx = Should.Throw<RetryException>(async () =>
            {
                result = await Retry.On<NullReferenceException, int>(() =>
                {
                    counter++;
                    throw new NullReferenceException();
                });
            });
            retryEx.RetryCount.ShouldBe(1);
            retryEx.Message.ShouldBe("retry failed after #1 attempts");

            counter.ShouldBe(2);
            result.ShouldBe(-1);
        }

        [Fact]
        public void WhenRetryResultTaskThat_does_not_fail_on_multiple_exceptions()
        {
            Should.NotThrow(async () =>
            {
                var counter = 0;
                var result = await Retry.OnAny<ArgumentNullException, NullReferenceException, int>(async () =>
                {
                    await Task.Yield();
                    counter++;
                    return 42;
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());

                counter.ShouldBe(1);
                result.ShouldBe(42);
            });
        }

        [Fact]
        public void WhenRetryResultTaskThat_fails_twice_but_succeeds_eventually_on_multiple_exceptions()
        {
            Should.NotThrow(async () =>
            {
                var counter = 0;
                var result = await Retry.OnAny<ArgumentNullException, NullReferenceException, int>(async () =>
                {
                    await Task.Yield();
                    if(counter++ < 2)
                    {
                        throw new NullReferenceException();
                    }

                    return 42;
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());

                counter.ShouldBe(3);
                result.ShouldBe(42);
            });
        }

        [Fact]
        public void WhenRetryResultTaskThat_always_fails_on_multiple_exceptions()
        {
            var result = -1;
            var counter = 0;
            var retryEx = Should.Throw<RetryException>(async () =>
            {
                result = await Retry.OnAny<ArgumentNullException, NullReferenceException, int>(() =>
                {
                    counter++;
                    throw new NullReferenceException();
                },
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());
            });
            retryEx.RetryCount.ShouldBe(3);
            retryEx.Message.ShouldBe("retry failed after #3 attempts");

            counter.ShouldBe(4);
            result.ShouldBe(-1);
        }

        [Fact]
        public void WhenRetryResultTaskThat_throws_aggregate_exception_one()
        {
            var counter = 0;

            var retryEx = Should.Throw<RetryException>(async () =>
            {
                await Retry.On<ArgumentNullException>(
                    () => Task.Factory.StartNew(() =>
                    {
                        counter++;
                        var inner = new ArgumentNullException("someArg");
                        throw new AggregateException(inner);
                        //return 1;
                    }),
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());
            });

            retryEx.RetryCount.ShouldBe(3);
            retryEx.InnerException.ShouldBeOfType<AggregateException>();
            retryEx.Message.ShouldBe("retry failed after #3 attempts");

            counter.ShouldBe(4);
        }

        [Fact]
        public void WhenRetryResultTaskThat_throws_aggregate_exception_two()
        {
            var counter = 0;

            var retryEx = Should.Throw<RetryException>(async () =>
            {
                await Retry.On<IndexOutOfRangeException>(
                    () => Task.Factory.StartNew(() =>
                    {
                        counter++;
                        var inner1 = new ArgumentNullException("someArg1");
                        var inner2 = new IndexOutOfRangeException("someArg2");
                        throw new AggregateException(inner1, inner2);
                        //return 1;
                    }),
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());
            });

            retryEx.RetryCount.ShouldBe(3);
            retryEx.InnerException.ShouldBeOfType<AggregateException>();
            retryEx.Message.ShouldBe("retry failed after #3 attempts");

            counter.ShouldBe(4);
        }

        [Fact]
        public void WhenRetryResultTaskThat_throws_aggregate_exception_and_no_expected_exception()
        {
            var counter = 0;

            Should.Throw<IndexOutOfRangeException>(async () =>
            {
                await Retry.On<ArgumentNullException>(
                    () => Task.Factory.StartNew(() =>
                    {
                        counter++;
                        var inner = new IndexOutOfRangeException();
                        throw new AggregateException(inner);
                        //return 1;
                    }),
                    100.Milliseconds(),
                    100.Milliseconds(),
                    100.Milliseconds());
            });

            counter.ShouldBe(1);
        }

        [Fact]
        public void WhenRetryResultTask_with_a_predicate_returning_true()
        {
            var predicateCounter = 0;

            Func<Exception, bool> exceptionPredicate = e =>
            {
                e.ShouldBeOfType<ArgumentException>();
                predicateCounter++;
                return true;
            };

            var executionCounter = 0;

            Func<Task<int>> task = () => Task.Run(() =>
            {
                executionCounter++;
                throw new ArgumentException();
#pragma warning disable CS0162 // Unreachable code detected
                return 1;
#pragma warning restore CS0162 // Unreachable code detected
            });

            var retryEx = Should.Throw<RetryException>(async () =>
            {
                await Retry.On(task, exceptionPredicate);
            });

            retryEx.RetryCount.ShouldBe(1);
            retryEx.InnerException.ShouldBeOfType<ArgumentException>();
            retryEx.Message.ShouldBe("retry failed after #1 attempts");

            executionCounter.ShouldBe(2);
            predicateCounter.ShouldBe(1);
        }

        [Fact]
        public void WhenRetryResultTask_with_a_predicate_returning_false()
        {
            var predicateCounter = 0;

            Func<Exception, bool> exceptionPredicate = e =>
            {
                e.ShouldBeOfType<ArgumentException>();
                predicateCounter++;
                return false;
            };

            var executionCounter = 0;
            Func<Task<int>> task = () => Task.Run(() =>
            {
                executionCounter++;
                throw new ArgumentException();
#pragma warning disable CS0162 // Unreachable code detected
                return 1;
#pragma warning restore CS0162 // Unreachable code detected
            });

            Should.Throw<ArgumentException>(async () =>
            {
                await Retry.On(task, exceptionPredicate);
            });

            executionCounter.ShouldBe(1);
            predicateCounter.ShouldBe(1);
        }

        [Fact]
        public void WhenRetryResultTask_with_delay_factory()
        {
            var cts = new CancellationTokenSource();

            var predicateCounter = 0;

            Func<Exception, bool> exceptionPredicate = e =>
            {
                e.ShouldBeOfType<ArgumentException>();
                predicateCounter++;
                return true;
            };

            Func<int, TimeSpan> delayFactory = failureCount =>
            {
                if(failureCount == 4)
                {
                    cts.Cancel();
                    return 0.Seconds();
                }

                return progressive(failureCount);
            };

            var executionCounter = 0;

            var retryEx = Should.Throw<RetryException>(async () =>
            {
                await Retry.On(async () =>
                {
                    await Task.Delay(1);
                    executionCounter++;
                    throw new ArgumentException();
#pragma warning disable CS0162 // Unreachable code detected
                    return 1;
#pragma warning restore CS0162 // Unreachable code detected
                }, exceptionPredicate, delayFactory, cts.Token);
            });

            retryEx.RetryCount.ShouldBe(3);
            retryEx.InnerException.ShouldBeOfType<ArgumentException>();
            retryEx.Message.ShouldBe("retry failed after #3 attempts");

            executionCounter.ShouldBe(4);
            predicateCounter.ShouldBe(4);
        }
    }
}