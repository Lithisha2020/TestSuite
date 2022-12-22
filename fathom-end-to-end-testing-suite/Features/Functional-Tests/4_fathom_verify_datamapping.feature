Feature: 4_fathom_verify_datamapping
	As a fathom client
	I want the ability to map my data to custom labels
	So that I can standardize the names for reporting

Background:
	Given an authentication token is available

@functional_test
Scenario: 1_RIO test case for data mapping
	Given existing dataset for <test_case_scenario>
	Given a variable data request is created for <request_variable_data>
	Given variable data request is completed
	When I get the variable data in an excel for <response_variable_data>
	When I update the data mapping
	When upload the data map file with <updated_datamap_loadfile_settings> and <updated_datamap_file>
	When the file is loaded with loaderId
	Then a variable data request is created for <request_variable_data_datamap_verification>
	Then variable data request is completed
	Then I should get the variables data for the data mapped variables for <response_variable_data_mapping>

	#Then I should get the variables data for the final namespace for <request_variable_final_namespace>
	Examples:
		| test_case_scenario      | request_variable_data                                   | response_variable_data                                  | updated_datamap_file                                    | updated_datamap_loadfile_settings   | request_variable_data_datamap_verification          | response_variable_data_mapping                          |
		| 1-2-file-cmplus-cmcomms | request_variable_data_wildcard_matching_csv_format.json | response_variable_data_wildcard_matching_csv_format.csv | response_variable_data_wildcard_matching_csv_format.csv | loadfilesettings_reload_datamap.txt | request_variable_data_wildcard_datamap_current.json | response_variable_data_wildcard_matching_csv_format.csv |

@functional_test
Scenario: 2_RIO test case for new data mapping
	Given a new dataset for <test_case_scenario> with <template_file> for <datafiles> and <loadsettingsfiles>
	Given I load data file and they are loaded

	#Given a variable data request is created for <request_variable_data>
	#Given variable data request is completed
	#When I get the variable data in an excel for <response_variable_data>
	#When I update the data mapping
	#When upload the data map file with <updated_datamap_loadfile_settings> and <updated_datamap_file>
	#When the file is loaded with loaderId
	#Then a variable data request is created for <request_variable_data_datamap_verification>
	#Then variable data request is completed
	#Then I should get the variables data for the data mapped variables for <response_variable_data_mapping>
	#Then I should get the variables data for the final namespace for <request_variable_final_namespace>
	Examples:
		| test_case_scenario                         | template_file  | datafiles                                                                                                                           | loadsettingsfiles | request_variable_data | response_variable_data | updated_datamap_file | updated_datamap_loadfile_settings | request_variable_data_datamap_verification | response_variable_data_mapping |
		| 1-3-files-survery-media-sales-search-rdata | FathomTemplate | Akzo Base Text Cube File_TL.csv,MEDIA Akzo Base Test File_TL.xlsx,SALES Akzo Base Test File.xlsx,SEARCH file Akzo Nobel UK.xlsx | Loadfilesettings_Akzo Base Text Cube File_TL.txt,Loadfilesettings_MEDIA Akzo Base Test File_TL.txt,Loadfilesettings_SALES Akzo Base Test File.txt,Loadfilesettings_SEARCH file Akzo Nobel UK.txt                  |request_variable_data_wildcard_matching_csv_format.json | response_variable_data_wildcard_matching_csv_format.csv | response_variable_data_wildcard_matching_csv_format.csv | loadfilesettings_reload_datamap.txt | request_variable_data_wildcard_datamap_current.json | response_variable_data_wildcard_matching_csv_format.csv |