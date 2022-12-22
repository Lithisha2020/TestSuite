Feature:7_fathom_verify_measure_datarequest
	As a fathom client
	I want the ability to create a measure data request  from a dataset 
	So that  data can be retrieved  for the given measures 

Background:
	Given an authentication token is available

@functional_test
Scenario: 1_verify Measure Data
	Given a new dataset for <test_case_scenario> with <template_file> for <file_name> and <file_settings>
	When I load data file
	And the file is loaded
   And  a measure data request is created for <request_measure_data>
	And measure data request is completed
	Then I should be able to see the measure data request as <response_measure_data>

	Examples:
		| test_case_scenario    | file_name     | template_file  | file_settings        | namespace |  request_measure_data | response measure_data |
		| 1-7-measure-data      | Example CM + Comms data (2).zip | fathomtemplate | LoadFileSettings.txt | test      |  request.json          | response.json         |
