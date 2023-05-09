import React, { useState } from 'react';
import CloseOutlinedIcon from '@material-ui/icons/CloseOutlined';
import { Grid, Typography, Button } from '@material-ui/core';
import { useSearchParams, useParams, useNavigate } from 'react-router-dom';

import { useFormBuilder } from 'redux/hooks';
import useStyles from './styles';

function FormSectionList() {
  const classes = useStyles();
  const { formId } = useParams();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { questionsGroups, updateQuestionsGroupsData, submitFormSectionData } = useFormBuilder();

  const questionsGroupsToShow = questionsGroups.filter(
    questionsGroup => questionsGroup.IsDeleted === false
  );

  const firstSectionQuestionGroupId = questionsGroupsToShow.length
    ? questionsGroupsToShow[0].QuestionGroupDataTypeId
    : -1;

  const questionGroup = searchParams.get('qg');

  const handleDelete = async key => {
    const index = questionsGroups.findIndex(
      questionGroup => questionGroup.QuestionGroupDataTypeId === key
    );

    await submitFormSectionData({
      FormSettings: { FormId: formId },
      FormQuestionGroup: { ...questionsGroups[index], IsDeleted: true },
      FormQuestion: null
    });
  };

  const addNewSectionHandler = () => {
    updateQuestionsGroupsData([
      ...questionsGroups,
      {
        ID: null,
        DisplayOrder: questionsGroups.length + 1,
        FormId: formId,
        IsDeleted: false,
        Name: 'New Section',
        QuestionGroupDataTypeId: 0
      }
    ]);
  };

  return (
    <Grid item lg={2} xs={12} sm={6} className={classes.sectionListWrapper}>
      <Typography
        variant="h6"
        className={classes.settingsLink}
        onClick={() => navigate(`/Org/form/edit/${formId}`)}
      >
        Settings
      </Typography>
      <Typography
        variant="h6"
        className={classes.sectionsListHeading}
        onClick={() => {
          setSearchParams(`qg=${firstSectionQuestionGroupId}`);
        }}
      >
        Sections
      </Typography>
      {questionsGroupsToShow.map(({ QuestionGroupDataTypeId: key, Name }) => (
        <Grid item key={key} spacing={2} className={classes.displayFlex}>
          <Grid lg={11}>
            <Typography
              variant="body1"
              className={questionGroup == key ? classes.currentSection : classes.sectionName}
              onClick={() => {
                setSearchParams(`qg=${key}`);
                // setSectionName(Name);
              }}
            >
              {Name}
            </Typography>
          </Grid>
          <Grid lg={1}>
            {key === 0 && (
              <CloseOutlinedIcon
                style={{ cursor: 'pointer' }}
                onClick={async () => {
                  await handleDelete(key);

                  if (questionGroup == key) {
                    if (questionsGroupsToShow.length > 0) {
                      setSearchParams(`qg=${firstSectionQuestionGroupId}`);
                    } else {
                      navigate(`/Org/form/edit/${formId}`);
                    }
                  }
                }}
              />
            )}
          </Grid>
        </Grid>
      ))}
      <Button
        color="primary"
        variant="outlined"
        style={{
          color: '#494949',
          fontSize: '14px',
          fontWeight: 'bold',
          border: '1px solid #E4E4E4'
        }}
        type="submit"
        disabled={false}
        onClick={event => {
          event.preventDefault();
          addNewSectionHandler();
        }}
        data-testid={`Back-${'Name'}`}
      >
        + Add New Section
      </Button>
    </Grid>
  );
}

export default FormSectionList;
