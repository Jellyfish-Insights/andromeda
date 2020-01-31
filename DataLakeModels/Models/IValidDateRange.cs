using System;

namespace DataLakeModels.Models {

    /**
        This interface is used to signal that the values that the model object
        holds are not valid anymore. This is a way to implement versioning.
        When a new version of the value is found, the previous version has its
        validity range closed.

        For instance:

        On day 0, object x is created with value y. So, x will have
        "ValidityStart == 0" and "ValidityEnd == MaxValue".!--

        Then on day 4, the value of object x is updated from y to z. So, the
        original x has its "ValidityEnd" set to 4, and the new x is created with
        "ValidityStart == 4" and "ValidityEnd == MaxValue".!--

        So, to find out the value of x as of date t, you need to use
        "where x.ValidityStart <= t && t < x.ValidityEnd"
     */
    public interface IValidityRange {
        DateTime ValidityStart { get; set; }
        DateTime ValidityEnd { get; set; }
    }
}
