import React from 'react';
import cs from 'classnames';
import CheckIcon from '@material-ui/icons/Check';
import { Grid, Box, Checkbox, makeStyles } from '@material-ui/core';
import ClickAwayListener from '@material-ui/core/ClickAwayListener';

import useStyles from './styles';

const RenderAudienceList = ({ setFieldValue, questionSettings, setShowAudience }) => {
  const classes = useStyles();
  const audienceTypes = ['All', 'Parent', 'Student', 'Adult'];

  const RenderAudienceRow = ({ audience: { Audience, Show, Required } }) => {
    const clickHandler = propertyName => {
      const updatedSettings = questionSettings.map(setting => {
        if (setting.Audience !== Audience) {
          return setting;
        }

        let updatedSetting = { ...setting };
        switch (propertyName) {
          case 'show':
            updatedSetting.Show = !setting.Show;
            updatedSetting.Required = setting.Show ? false : setting.Required;
            break;
          case 'required':
            updatedSetting.Required = !setting.Required;
            updatedSetting.Show = !setting.Required ? true : setting.Show;
            break;
          default:
            break;
        }

        return updatedSetting;
      });

      setFieldValue(updatedSettings);
    };

    return (
      <Grid container row key={Audience} className={classes.audienceRow}>
        <Grid item lg={4} xs={4} sm={4}>
          {audienceTypes[Audience]}
        </Grid>
        <Grid item lg={4} xs={4} sm={4}>
          <RenderCheckbox checked={Show} handleCheck={() => clickHandler('show')} />
        </Grid>
        <Grid item lg={4} xs={4} sm={4}>
          <RenderCheckbox checked={Required} handleCheck={() => clickHandler('required')} />
        </Grid>
      </Grid>
    );
  };

  return (
    <ClickAwayListener onClickAway={() => setShowAudience(false)}>
      <Box className={cs(classes.positionAbsolute, classes.audienceList)}>
        <Grid container>
          <Grid container row className={classes.audienceRow}>
            <Grid item lg={4} xs={4} sm={4}></Grid>
            <Grid item lg={4} xs={4} sm={4}>
              <b>Show</b>
            </Grid>
            <Grid item lg={4} xs={4} sm={4}>
              <b>{'Require (*)'}</b>
            </Grid>
          </Grid>
          {questionSettings.map(audience => {
            return <RenderAudienceRow audience={audience} key={audience.Audience} />;
          })}
          <RenderAudienceRow audience={{ Audience: 0, Show: false, Required: false }} key={0} />
        </Grid>
      </Box>
    </ClickAwayListener>
  );
};

const RenderCheckbox = ({ checked = false, handleCheck }) => {
  const classes = makeStyles(() => ({
    checkboxWrapper: {
      padding: '0px 12px'
    },
    formControlLabelRoot: {
      margin: '10px 0px',
      '& .MuiTypography-root': {
        marginLeft: 18
      }
    },
    checked: {
      color: '#494949 !important',
      padding: 0,
      borderRadius: 3,
      background: 'transparent !important'
    },
    root: {
      padding: 0,
      background: '#ffffff',
      color: '#ffffff',
      border: '1px solid #c4c4c4',
      borderRadius: 3,
      height: '18px',
      width: '18px',
      '& .MuiSvgIcon-root': {
        height: '18px',
        width: '18px'
      }
    }
  }))();

  return (
    <Checkbox
      data-testid={`${'title'}-${'QuestionDataTypeId'}-checkbox`}
      checked={checked}
      onChange={handleCheck}
      classes={{
        checked: classes.checked,
        root: classes.root
      }}
      checkedIcon={
        <CheckIcon
          style={{
            height: '18px',
            width: '18px'
          }}
        />
      }
      IconStyle={{ fill: 'black' }}
    />
  );
};

export default RenderAudienceList;
