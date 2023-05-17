import React from 'react';
import cs from 'classnames';
import CheckIcon from '@material-ui/icons/Check';
import { Grid, Box, Checkbox, makeStyles } from '@material-ui/core';
import ClickAwayListener from '@material-ui/core/ClickAwayListener';

import useStyles from './styles';

const RenderAudienceList = ({ setFieldValue, questionSettings, setShowAudience, dataTestId }) => {
  const classes = useStyles();
  const audienceTypes = ['All', 'Parent', 'Student', 'Adult'];

  const RenderAudienceRow = ({ audience: { Audience, Show, Required } }) => {
    const clickHandler = (propertyName, val) => {
      const updatedAllSettings = questionSettings.map(setting => {
        if (Audience !== 0 && setting.Audience !== Audience) {
          return setting;
        }

        let updatedSetting = { ...setting };
        switch (propertyName) {
          case 'show':
            updatedSetting.Show = Audience === 0 ? val : !setting.Show;
            updatedSetting.Required = updatedSetting.Show ? setting.Required : false;
            break;
          case 'required':
            updatedSetting.Required = Audience === 0 ? val : !setting.Required;
            updatedSetting.Show = updatedSetting.Required ? true : setting.Show;
            break;
          default:
            break;
        }

        return updatedSetting;
      });

      setFieldValue(updatedAllSettings);
    };

    return (
      <Grid container row key={Audience} className={classes.audienceRow}>
        <Grid item lg={4} xs={4} sm={4}>
          {audienceTypes[Audience]}
        </Grid>
        <Grid item lg={4} xs={4} sm={4}>
          <RenderCheckbox
            checked={Show}
            dataTestId={`showCheckBox${Audience}-${dataTestId}`}
            handleCheck={event => clickHandler('show', event.target.checked)}
          />
        </Grid>
        <Grid item lg={4} xs={4} sm={4}>
          <RenderCheckbox
            checked={Required}
            dataTestId={`requiredCheckBox${Audience}-${dataTestId}`}
            handleCheck={event => clickHandler('required', event.target.checked)}
          />
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
          <RenderAudienceRow
            audience={{
              Audience: 0,
              Show: questionSettings.every(q => q.Show),
              Required: questionSettings.every(q => q.Required)
            }}
            key={0}
          />
          {questionSettings.map(audience => {
            return <RenderAudienceRow audience={audience} key={audience.Audience} />;
          })}
        </Grid>
      </Box>
    </ClickAwayListener>
  );
};

const RenderCheckbox = ({ checked = false, handleCheck, dataTestId }) => {
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
      data-testid={`${dataTestId}-checkbox`}
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
