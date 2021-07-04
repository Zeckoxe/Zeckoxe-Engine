﻿// Copyright (c) 2019-2021 Faber Leonardo. All Rights Reserved. https://github.com/FaberSanZ
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

namespace Vultaik
{
    public class DeviceExtension
    {
        public DeviceExtension()
        {

        }

        public DeviceExtension(string name, bool support)
        {
            Name = name;
            Support = support;
        }

        public string Name { get; set; }
        public bool Support { get; set; }

    }
}