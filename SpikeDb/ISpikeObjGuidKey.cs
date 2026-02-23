namespace SpikeDb;

public interface ISpikeObj{}

public interface ISpikeObjGuid : ISpikeObj
{
    Guid Id { get; }
}

public interface ISpikeObjIntKey : ISpikeObj
{
    int Id { get; set; }
}

public static class SpikeObjExtensions
{
    extension<T>(T obj) where T : class, ISpikeObjIntKey
    {
        /// <summary>
        /// Pipes obj into the first result of SpikeRepo.ReadCollection<TR>() where finder(obj,x) == true
        /// This version is nicer to read / write, but it reads all of T into memory
        /// </summary>
        /// <param name="finder">A predicate function to find a single TR</param>
        /// <typeparam name="TR">The Result Type</typeparam>
        /// <returns>A single TR object, or throws</returns>
        public TR Then<TR>(Func<T,TR,bool> finder) where TR : class, ISpikeObjIntKey
        {
            return SpikeRepo
                .ReadCollection<TR>() // todo fix, dont read all into mem
                .Single(x => finder(obj, x));
        }

        // This version is harder to read / write, but it doesn't read all of T into memory'
        public TR Then2<TP, TR>(Func<T, TP> propFinder, Func<TR, TP, bool> resultFinder ) where TR : class, ISpikeObjIntKey
        {
            var prop = propFinder(obj);

            bool Predicate(TR o) => resultFinder(o, prop);
            
            return SpikeRepo
                .ReadCollection<TR>(Predicate) // todo fix, dont read all into mem
                .Single();
        }
    }
    
}