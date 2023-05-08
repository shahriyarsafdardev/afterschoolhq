import React, { useState } from 'react';
import CloseOutlinedIcon from '@material-ui/icons/CloseOutlined';
import { Grid, Typography, Button } from '@material-ui/core';
import { useSearchParams, useParams } from 'react-router-dom';

import { useFormBuilder } from 'redux/hooks';
import useStyles from './styles';

function FormSectionList() {
  const classes = useStyles();
  //   const [sectionName, setSectionName] = useState('Settings');

  const [searchParams, setSearchParams] = useSearchParams();

  const questionGroup = searchParams.get('qg');
  const { questionsGroups } = useFormBuilder();
  return (
    <Grid item lg={2} xs={12} sm={6} className={classes.sectionListWrapper}>
      <Typography variant="h6" className={classes.settingsLink}>
        Settings
      </Typography>
      <Typography variant="h6" className={classes.sectionsListHeading}>
        Sections
      </Typography>
      {questionsGroups.map(({ QuestionGroupDataTypeId: key, Name }) => (
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
            <CloseOutlinedIcon />
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
        }}
        data-testid={`Back-${'Name'}`}
      >
        + Add New Section
      </Button>
    </Grid>
  );
}

export default FormSectionList;
