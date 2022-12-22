Feature: 6_fathom_verify_snapshot_creation
	As a fathom client
	I want the ability to create a snapshot from a group 
	So that the snapshot data can be retrieved 

Background:
	Given an authentication token is available

@functional_test
Scenario: 1_Check Snapshot is Created
	Given a new dataset for <test_case_scenario> with <template_file> for <file_name> and <file_settings>
	When I load data file
	And the file is loaded
	And the data is imported in namespace <namespace>
	And a group <group_name> is created with <group_variables>
	And a snapshot <snapshot_name> of the group <group_name> is created
	And a variable data request is created for <request_variable_data>
	And variable data request is completed
	Then I should be able to see the variable data request as <response_variable_data>

	Examples:
		| test_case_scenario    | file_name     | template_file  | file_settings        | namespace | group_name | group_variables     | snapshot_name | request_variable_data | response_variable_data |
		| 1-5-snapshot-creation | data_file.csv | FathomTemplate | LoadFileSettings.txt | test      | test_group | group_variables.txt | v01           | request.json          | response.json          |
