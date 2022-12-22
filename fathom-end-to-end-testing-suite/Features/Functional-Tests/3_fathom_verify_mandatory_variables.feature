Feature: 3_fathom verify mandatory variables
	As a fathom user
	I want the ability to upload a dataset
	So that I can get the mandatory variables

Background:
	Given an authentication token is available

@functional_test
Scenario Outline: 1_Simple Test
	Given a new dataset for <test_case_scenario> with <template_file> for <file_name> and <file_settings>
	When I load data file
	And the file is loaded
	Then I should see all the variables from <response_all_variables>

	Examples:
		| test_case_scenario      | file_name                     | template_file  | file_settings        | response_all_variables      |
		| 1-2-file-cmplus-cmcomms | Example_CMplus_Comms_data.zip | FathomTemplate | loadfilesettings.txt | response_all_variables.json |

@functional_test
Scenario Outline: 2_Existing Dataset Test
	Given existing dataset for <test_case_scenario>
	Then I should see all the variables from <response_all_variables>

	Examples:
		| test_case_scenario      | file_name                     | template_file  | file_settings        | response_all_variables      |
		| 1-2-file-cmplus-cmcomms | Example_CMplus_Comms_data.zip | FathomTemplate | loadfilesettings.txt | response_all_variables.json |

@functional_test
Scenario Outline: 3_To get a particular variable data
	Given existing dataset for <test_case_scenario>
	Given a variable data request is created for <request_variable_data>
	Then variable data request is completed
	Then I should be able to see the variable data request as <response_variable_data>

	Examples:
		| test_case_scenario      | file_name                     | template_file  | file_settings        | request_variable_data                      | response_variable_data                      |
		| 1-2-file-cmplus-cmcomms | Example_CMplus_Comms_data.zip | FathomTemplate | loadfilesettings.txt | request_variable_data_single_variable.json | response_variable_data_single_variable.json |