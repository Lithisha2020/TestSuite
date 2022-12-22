Feature: 5_fathom_hbg_leading_and_trailing_spaces_removed
	As a fathom client
	I want the ability to check if leading/trailing spaces have been removed from predefined variables 
	So that the data is correctly formatted for those variables 

Background:
	Given an authentication token is available

@functional_test
Scenario: 1_Check Leading and Trailing Spaces are Removed
	Given an existing minimum package version <minimum_version>
	Given a new dataset for <test_case_scenario> with <template_file> for <file_name> and <file_settings>
	Given a test package <test_package> for <test_case_scenario> has been applied
	When I load data file
	And the file is loaded
	And a variable data request is created for <request_variable_data>
	And variable data request is completed
	Then total blanks in HBG variables should be 0

	Examples:
		| test_case_scenario                  | file_name                              | template_file  | file_settings                                           | minimum_version | test_package              | request_variable_data |
		| 1-4-hbg-leading-and-trailing-spaces | Automotive Debranded Cube_v2_small.csv | FathomTemplate | Loadfilesettings_Automotive Debranded Cube_v2_small.txt | HBG.2.826       | BlankSpacesCheck.1.0.json | request.json          |