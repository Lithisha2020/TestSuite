Feature: 1_Fathom new dataset creation basic flow
	As a fathom end user
	I want the ability to upload a new dataset
	So that I can can get analytics datapoints

Background:
	Given an authentication token is available

@functional_test
Scenario Outline: Upload new dataset
	Given a new dataset for <test_case_scenario> with <template_file> for <file_name> and <file_settings>
	When I load data file
	And the file is loaded
	Then should be able to measure data for variables

	Examples:
		| test_case_scenario | file_name    | template_file  | file_settings        |
		| 1-1-file-com-basic | datamap.xlsx | FathomTemplate | loadfilesettings.txt |