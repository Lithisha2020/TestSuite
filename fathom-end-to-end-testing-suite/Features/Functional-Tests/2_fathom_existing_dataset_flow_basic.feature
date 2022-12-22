Feature: 2_Fathom existing dataset creation basic flow
	As a fathom end user
	I want the ability to use an existing dataset
	So that I can can get analytics datapoints

Background:
	Given an authentication token is available

@functional_test
Scenario Outline: Upload to existing dataset
	Given an existing dataset for <test_case_scenario> with <file_name> and <file_settings>
	When I load data file
	And the file is loaded
	Then should be able to measure data for variables

	Examples:
		| test_case_scenario | file_name    | file_settings        |
		| 1-1-file-com-basic | datamap.xlsx | loadfilesettings.txt |