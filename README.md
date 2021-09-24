#  Entity Framework Core - QueryFilterBuilder

This library solves a certain problem shown in the image below. This library adds an extension method to the `EntityTypeBuilder` class that allows the use of `HasQueryFilter()` by passing to it an instance of the `QueryFilterBuilder` class also provided by this library. The class allows you to combine multiple rules/filters and then by calling the `Build()` method, combining them into one filter (LINQ expression predicate) that can be used in the `HasQueryFilter()` method.

![Zrzut ekranu 2021-09-24 162020](https://user-images.githubusercontent.com/46250989/134690210-3d43fa52-2f72-4596-8432-8b6234b107f9.png)

The method of adding a filter has a bool parameter that indicates whether the filter should be used or not. Thanks to this, any service injected into DbContext can control which filters should be applied in a given scope.

##  Example
An example of using `QueryFilterBuilder` in the override  `OnModelCreating()` method in the `DbContext` class.

	protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
	    //Fluent API
	    modelBuilder.Entity<Blog>()
	        .HasQueryFilter(QueryFilterBuilder<Blog>
	            .Create()
                .AddFilter(b => b.Name == "Hello World")
                .AddFilter(b => b.Posts == 20, _injectedService.ShouldApplyFilter())
                .Build());

		//As a last command, you can call Build(), 
		//but you don't have to because it will be called automatically.
    }
  