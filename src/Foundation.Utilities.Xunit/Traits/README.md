``` 
xunit.console.exe ... -trait "Category=UnitTest"
dotnet test --filter "Category=UnitTest" 

```

or 
``` 
xunit.console.exe ... -trait "Bug=123"
dotnet test --filter "Bug=123" 
```

more info: http://www.brendanconnolly.net/organizing-tests-with-xunit-traits/