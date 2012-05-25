#region Copyright & licence

// This file is part of NinjaTurtles.
// 
// NinjaTurtles is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// NinjaTurtles is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with Refix.  If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 David Musgrove and others.

#endregion

using System;
using System.Collections;
using System.Linq;

using Mono.Cecil;

namespace NinjaTurtles
{
    internal class MethodDefinitionResolver
    {
        public static MethodDefinition ResolveMethod(TypeDefinition typeDefinition, string methodName)
        {
            try
            {
                return typeDefinition.Methods.Single(m => m.Name == methodName);
            }
            catch
            {
                return null;
            }
        }

        public static MethodDefinition ResolveMethod(TypeDefinition typeDefinition, string methodName, Type[] parameterTypes)
        {
            if (parameterTypes == null)
            {
                return ResolveMethod(typeDefinition, methodName);
            }
            try
            {
                return typeDefinition.Methods.Single(m => m.Name == methodName
                                                          &&
                                                          Enumerable.SequenceEqual(
                                                              m.Parameters.Select(p => p.ParameterType.Name.Replace("TypeDefinition", "Type")),
                                                              parameterTypes.Select(p => p.Name)));
            }
            catch
            {
                return null;
            }
        }
    }
}
