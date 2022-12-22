# What is fathom end to end testing integration suite?
Fathom end to end testing integration suite is a set of system integration tests and functional tests. This is an independent test suite that can be run against any Fathom environment and check its integration boundaries and functional health for that environment.

## System Integration Test:
 Basic test case that just checks if the health of the integrating boundaries of Fathom are working. It checks if the responses are available. It is a quick test.
 Fathom system integration boundaries covered:
 1. SSO endpoint
 2. Fathom web api availability
 3. Fathom database

 ## Functional Test:
 Fathom external interface is an API that has ability to perform several operations to form a workflow of event. In general the workflow can be defined as:
 1. Dataset ingestion
 2. Processing of this dataset by Fathom
 3. Returning results of the processed data

 Each process defined above is exposed by several endpoints. Functional tests cover the functionality that is exposed by the endpoints. Each test case is an independent unit that does the set up for the test, getting the response from the endpoint and then cleaning up the resources.

 It performs black box testing.

## What is NOT part of fathom end to end testing suite integration suite?
Fathom does not test using mocks or stubs. It works with pointing to Fathom endpoint. 
It does not perform white box testing.

# How to run the test suite?
## Technologies and references:
1. Visual studio 2019(Enterprise edition needed to associate tests to the ADO test case)
2. Installation of specflow engine to integrate .feature files in Visual Studio 2019. In visual studio goto Extensions> Search for "SpecFlow for Visual Studio 2019" and install it. It will require to restart visual studio.
3. Nuget dependencies as mentioned in packages.config
4. Clone this repository and open the solution in Visual Studio 2019
    
##	Compilation process:
1. Update the fathom.runsettings file with the appropriate environment credentials.
2. Compile the solution. There should be no compilation error.
3. You should now see tests in Test Explorer window.

##	Run the test suite:
To run the test suite, go to Test Explorer window and select the test you want to run or  the root fathom-end-to-end-testing-suite and rightclick > Run.

##	How to read the test results:
Test results are available in the group summary section of Test Explorer. 

# How to add new test cases:
1. To add Functional Tests:
    1. Determine the acceptance criteria in Behavioral Driven Design format of Given, When and Then. Refer [BDD](https://www.bddtesting.com/) for more information.
    2. Add a feature file by right clicking in Features>Functional-Tests folder and selecting Specflow>Features file.
    3. Add the acceptance criteria.
    4. Go to steps folder and right click to add new item. Select specflow>Specflow Step Definition file. This will add .cs file
    5. Go back to the feature file created earlier and right click and select Generate Step Defintions. Copy the result to the .cs file.
    6. For each method generated in .cs file, follow this pattern to validate each step:
          - Arrange
          - Act
          - Assert
    7. The same test case can be run by different input parameters. The resources needed for input parameters and output result for those parameters should be kept in the **Resources** folder. Name the resource folder appropriately. Naming convention is important to understand what type of test data is contained:
         **<TypeOfTestCase>_<SerialNumberOfTestCase>_WhatItContains**

         **TypeOfTestCase**: Functional Test = 1, 
                System Integration Test = 2
         **SerialNumberOfTestCase**: This is running number to track how much test data is available in each type of test case.
       
       The test data folder should be independent of any other dependencies else where. It is a self sufficient parameter set. It can contain request and responses for the test data in the folder. It can contain several requests and responses in different test conditions but all of them should be relevant only to the test data in this folder. 
        
        This self containment is needed so that when regression test case fails for a particular test data then only a particular folder can be checked/debugged without disturbing other test cases.



# FAQ:
