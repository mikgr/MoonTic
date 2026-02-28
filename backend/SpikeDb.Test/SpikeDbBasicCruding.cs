namespace SpikeDb.Test;


class Animal : ISpikeObjGuid
{
    public Guid Id { get; set; }
    public required string Name { get; init; }
}

class Dog : Animal
{
    private int _ageInDogYears = 0;
    public string Breed { get; set; } = "";

    public void SetAgeInHumanYears(int age)
    {
        _ageInDogYears = age * 7;
    }
    public int AgeInDogYears => _ageInDogYears;
}


public class SpikeDbBasicCruding
{
    // support int id
    // support guid id - do not accept empty guid default
    // enums must work
    // collection prop must work


    [Fact]
    public void Persist_will_write_object_to_disk()
    {
        SpikeRepo.DangerousDeleteAllWithOutRecover<Animal>();
        
        new Animal { Id = Guid.NewGuid(), Name = "Fido" }.SpikePersist();
        var animals = SpikeRepo.ReadCollection<Animal>();
        Assert.Equal("Fido", animals.First().Name);
    }

    [Fact]
    public void Count_will_return_number_of_objects_on_disk()
    {
        // Arrange
        SpikeRepo.DangerousDeleteAllWithOutRecover<Animal>();
        new Animal { Id = Guid.NewGuid(), Name = "Fido" }.SpikePersist();
        new Animal { Id = Guid.NewGuid(), Name = "King" }.SpikePersist();
        
        // Act
        int count = SpikeRepo.Count<Animal>();
        
        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void DangerousDeleteAllWithOutRecover_will_delete_all_objects_from_disk()
    {
        // Arrange
        new Animal { Id = Guid.NewGuid(), Name = "Fido" }.SpikePersist();
        
        // Act
        SpikeRepo.DangerousDeleteAllWithOutRecover<Animal>();
        
        // Assert
        var count = SpikeRepo.Count<Animal>();
        Assert.Equal(0, count);
    }

    [Fact]
    public void ReadById_will_read_object_from_disk()
    {
        var testName = "name_ReadById";
        
        var animal = new Animal { Id = Guid.NewGuid(), Name = testName }.SpikePersist();
        var animalRead = SpikeRepo.ReadOrNullByGuid<Animal>(animal.Id);
        if (animalRead is null) throw new ArgumentNullException(nameof(animalRead));
        Assert.Equal(testName, animalRead.Name);
    }
    
    [Fact]
    public void ReadOrNullByGuid_will_return_null_if_object_not_found()
    {
        var animal = new Animal { Id = Guid.NewGuid(), Name = "name_ReadById" }.SpikePersist();
        var maybeRedAnimal = SpikeRepo.ReadOrNullByGuid<Animal>(Guid.NewGuid());
        
        Assert.Null(maybeRedAnimal);
    }
    
    [Fact]
    public void Two_level_inheritance_will_work()
    {
        // Arrange
        SpikeRepo.DangerousDeleteAllWithOutRecover<Dog>();
        
        var testName = "name_ReadById";
        var testBreed = "Beagle";
        
        // Act
        var animal = SpikeRepo.Persist(new Dog { Id = Guid.NewGuid(), Name = testName, Breed = testBreed });
        var animalRead = SpikeRepo.ReadOrNullByGuid<Dog>(animal.Id);
        if (animalRead is null) throw new ArgumentNullException(nameof(animalRead));
        
        // Assert
        Assert.Equal(testName, animalRead.Name);
        Assert.Equal(testBreed, animalRead.Breed);
    }
    
    [Fact]
    public void Private_fields_will_be_persisted_and_read()
    {
        // Arrange
        SpikeRepo.DangerousDeleteAllWithOutRecover<Dog>();
        
        var dog = new Dog { Id = Guid.NewGuid(), Name = "name_ReadById", Breed = "Beagle" };
        dog.SetAgeInHumanYears(5);
        
        // Act
        dog.SpikePersist();
        var animalRead = SpikeRepo.ReadOrNullByGuid<Dog>(dog.Id);
        if (animalRead is null) throw new ArgumentNullException(nameof(animalRead));
 
        // Assert
        Assert.Equal(35, animalRead.AgeInDogYears);

    }

    class Order : ISpikeObjGuid
    {
        public Guid Id { get; set; }
        public List<OrderLine> OrderLines { get; set; } = new();
    }

    class OrderLine
    {
        public required string Text { get; init; }
        public decimal Amount { get; set; }
    }

    [Fact]
    public void Collection_properties_will_be_persisted_and_read()
    {
        SpikeRepo.DangerousDeleteAllWithOutRecover<Order>();
        
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderLines = new()
            {
                new() { Text = "Line 1", Amount = 100m },
                new() { Text = "Line 2", Amount = 200m }
            }
        };

        // Act
        order.SpikePersist();
        var orderRead = SpikeRepo.ReadOrNullByGuid<Order>(order.Id);
        
        // Assert
        Assert.Equal(2, orderRead?.OrderLines.Count);
        Assert.Equal("Line 1", orderRead?.OrderLines[0].Text);
        Assert.Equal("Line 2", orderRead?.OrderLines[1].Text);
    }

    class Person : ISpikeObjIntKey
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
    }
    
    [Fact]
    public void Int_key_can_increment()
    {
        // Clean
        SpikeRepo.DangerousDeleteAllWithOutRecover<Person>();
        SpikeRepo.Truncate<Person>();
        
        // Arrange
        const string adaName = "ada";
        const string bobName = "bob";
        
        // Act
        var ada = new Person { Name = adaName, Id = -1 }.SpikePersistInt();
        var adaReadBack = SpikeRepo.ReadIntId<Person>(ada.Id);
        
        var bob = new Person { Name = bobName, Id = -1 }.SpikePersistInt();
        var bobReadBack = SpikeRepo.ReadIntId<Person>(bob.Id);
        
        // Assert
        Assert.Equal(0, adaReadBack.Id);
        Assert.Equal(adaName, adaReadBack.Name);
        
        Assert.Equal(1, bobReadBack.Id);
        Assert.Equal(bobName, bobReadBack.Name);
    }
    
    [Fact]
    public void Read_collection_filter_works()
    {
        // Clean
        SpikeRepo.DangerousDeleteAllWithOutRecover<Person>();
        SpikeRepo.Truncate<Person>();
        
        // Arrange
        const string adaName = "ada";
        const string bobName = "bob";
        
        // Act
        new Person { Name = adaName, Id = -1 }.SpikePersistInt();
        new Person { Name = bobName, Id = -1 }.SpikePersistInt();
        
        var persons = SpikeRepo.ReadCollection<Person>(filter: x => x.Name == bobName).ToList();
        
        // Assert
        Assert.Single(persons);
    }

    class User : ISpikeObjIntKey
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    class Account : ISpikeObjIntKey
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
    }
    
    [Fact]
    public void SpikeThen_will_pipe_the_result_of_iSpikeObj_into_then()
    {
        SpikeRepo.DangerousDeleteAllWithOutRecover<User>();
        SpikeRepo.DangerousDeleteAllWithOutRecover<Account>();
        
        var ada = new User{ Id = -1 }.SpikePersistInt();
        var bob = new User{ Id = -1 }.SpikePersistInt();
        var adaAccount = new Account{ Id = -1, UserId = ada.Id }.SpikePersistInt();
        var bobAccount = new Account{ Id = -1, UserId = bob.Id }.SpikePersistInt();

        var bobAccountRead = SpikeRepo
            .ReadIntId<User>(bob.Id)
            .Then((User usr, Account acc) => acc.UserId == usr.Id );

        Assert.Equal(bobAccount.Id, bobAccountRead.Id);
        
        var bobAccountRead2 = SpikeRepo
            .ReadIntId<User>(bob.Id)
            .Then2(u => u.Id, (Account a, int p) => a.UserId == p);
        
        Assert.Equal(bobAccount.Id, bobAccountRead2.Id);
    }
    
    [Fact]
    public void ReadSingleBy()
    {
        SpikeRepo.DangerousDeleteAllWithOutRecover<User>();
        
        var ada = new User{ Id = -1, Name = "ada" }.SpikePersistInt();
        var bob = new User{ Id = -1, Name = "bob" }.SpikePersistInt();

        var bobRead = SpikeRepo.ReadSingle<User>(by: x => x.Name == "bob");
        
        Assert.NotSame(bob, bobRead);
        Assert.Equal(bob.Id, bobRead.Id);
    }
    
    
}