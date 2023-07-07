import React from 'react';
import { Grid, Typography, Box } from '@material-ui/core';

import useStyles from './styles';
import { infoText } from './helper';
import { audienceTypes } from 'constants';
import RenderCheckbox from './RenderCheckbox';

const ApplyToAllControls = props => {
  const classes = useStyles();
  const { values, setValues, reRender, setReRender, questionGroup, settingsForAll } = props;
  const handleSettingForAll = (propertyName, registrantType, val) => {
    const updatedQuestionSettings = settings => {
      let updatedSettings = {};
      const audienceType = audienceTypes.indexOf(registrantType);

      if (audienceType === 0) {
        updatedSettings = settings.map(setting => {
          let updatedSetting = { ...setting };
          switch (propertyName) {
            case 'show':
              updatedSetting.Show = val;
              updatedSetting.Required = val ? setting.Required : false;
              break;
            case 'required':
              updatedSetting.Required = val;
              updatedSetting.Show = val ? true : setting.Show;
              break;
            default:
              break;
          }

          return updatedSetting;
        });
      } else {
        updatedSettings = settings.map(setting => {
          if (setting.Audience !== audienceTypes.indexOf(registrantType)) {
            return setting;
          }

          let updatedSetting = { ...setting };
          switch (propertyName) {
            case 'show':
              updatedSetting.Show = val;
              updatedSetting.Required = val ? setting.Required : false;
              break;
            case 'required':
              updatedSetting.Required = val;
              updatedSetting.Show = val ? true : setting.Show;
              break;
            default:
              break;
          }

          return updatedSetting;
        });
      }
      return updatedSettings;
    };
    const newValues = { ...values };

    Object.keys(newValues).forEach(key => {
      if (newValues[key].IsLocked) {
        return;
      }

      newValues[key] = {
        ...newValues[key],
        QuestionSettings: updatedQuestionSettings(newValues[key].QuestionSettings)
      };
    });

    setReRender(!reRender);
    setValues(newValues, false);
  };

  return (
    <Box>
      <Typography variant="h6" className={classes.applyAllHeading}>
        Apply to all in this section:
      </Typography>
      <Typography variant="body2" className={classes.infoText}>
        {infoText}
      </Typography>
      <Grid container className={classes.controlsContainer}>
        <Grid container item lg={12} sm={12} xs={12} className={classes.formControlsRow}>
          <Grid item lg={3} md={3} sm={3} xs={3}>
            <Typography variant="body1" className={classes.applyAllRow}>
              Show For:
            </Typography>
          </Grid>
          <Grid container item lg={9} sm={9} xs={9} className={classes.applyToAllControlsContainer}>
            {audienceTypes.map((key, i) => (
              <Grid
                item
                lg={3}
                md={3}
                sm={6}
                xs={6}
                key={key}
                className={classes.applyToAllControlsItem}
              >
                <RenderCheckbox
                  key={key}
                  checked={settingsForAll?.show[i]}
                  handleCheck={event => {
                    handleSettingForAll('show', key, event.target.checked);
                  }}
                  dataTestId={`ApplyAll-${questionGroup}-${key}-show`}
                />
                <Typography variant="body1" className={classes.applyToAllControlsWrapper}>
                  {key}
                </Typography>
              </Grid>
            ))}
          </Grid>
        </Grid>
        <Grid container item lg={12} sm={12} xs={12} className={classes.formControlsRow}>
          <Grid item lg={3} sm={3} xs={3}>
            <Typography variant="body1" className={classes.applyAllRow}>
              Required For:
            </Typography>
          </Grid>
          <Grid container className={classes.applyToAllControlsContainer}>
            {audienceTypes.map((key, i) => (
              <Grid
                item
                lg={3}
                md={3}
                sm={6}
                xs={6}
                key={key}
                className={classes.applyToAllControlsItem}
              >
                <RenderCheckbox
                  key={key}
                  checked={settingsForAll?.required[i]}
                  handleCheck={event => {
                    handleSettingForAll('required', key, event.target.checked);
                  }}
                  dataTestId={`ApplyAll-${questionGroup}-${key}-required`}
                />
                <Typography variant="body1" className={classes.applyToAllControlsWrapper}>
                  {key}
                </Typography>
              </Grid>
            ))}
          </Grid>
        </Grid>
      </Grid>
    </Box>
  );
};

export default ApplyToAllControls;
