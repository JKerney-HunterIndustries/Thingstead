namespace Thingstead.Engine.Tests.EmptyStep

open Thingstead.Engine.Tests
open Thingstead.Types

module NeedsToRun = 
        let private path = Some "Thingstead Test Engine 'EmptyStep' should"

        let private template = 
            { testTemplate with
                Path = path
            }

        let private testedWith = applyToTemplate template

        let tests : Test list = 
            [
            ]