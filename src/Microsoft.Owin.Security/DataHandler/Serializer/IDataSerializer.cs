// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.DataHandler.Serializer
{
    public interface IDataSerializer<TModel>
    {
        byte[] Serialize(TModel model);
        TModel Deserialize(byte[] data);
    }
}
