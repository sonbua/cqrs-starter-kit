﻿using System;

namespace Cafe.Commands.Tab
{
    public class TabNotOpen : Exception
    {
    }

    public class DrinksNotOutstanding : Exception
    {
    }

    public class FoodNotOutstanding : Exception
    {
    }

    public class FoodNotPrepared : Exception
    {
    }

    public class MustPayEnough : Exception
    {
    }

    public class TabHasUnservedItems : Exception
    {
    }
}
