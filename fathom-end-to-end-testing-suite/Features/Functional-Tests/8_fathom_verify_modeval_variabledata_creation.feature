Feature: 8_fathom verify fathom modeval variable creation
	As a fathom user
	I want the ability to upload a dataset
	So that I can get VDR

Background:
	Given an authentication token is available

@functional_test
Scenario Outline: Loading rio modeval_VDR
	Given a new dataset for <test_case_scenario> with <template_file> for <file_name> and <file_settings>
	When I load data file
	And the file is loaded
	When I create vdr request for<request_modeval_file>
	Then verify vdr should be success
	And Vdr response should be stored in <response_modeval_file>

	Examples:
		| test_case_scenario	|	file_name				|	template_file	|	file_settings					| request_modeval_file		|response_modeval_file      |
		| 1-8-fathom-modeval	| 83515 - Fathom_input.zip	| FathomTemplate	| Loadfilesettings_rio_modeval.txt	| request_rio_modeval.json	| response_rio_modeval.json |