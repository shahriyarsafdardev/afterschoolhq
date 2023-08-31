import cs from 'classnames';
import useStyles from './styles';
import React, { useState, useEffect } from 'react';
import { Typography, Box } from '@material-ui/core';
import { locationObject } from 'components/SchoolField/SchoolField';
import { GoogleLocation } from 'components';

function ApiTextField({ formik, question, currentQuestions }) {
  const classes = useStyles();
  const [disabled, setDisable] = useState(true);
  const { touched, values, errors, setFieldValue } = formik;
  const { ID, Text, IsRequired: required = false, QuestionDataTypeId } = question;
  const [typeClass, typeText] = required
    ? [classes.fieldLabelRequired, '*']
    : [classes.fieldLabelOptional, '(Optional)'];

  const setAddressFields = place => {
    currentQuestions.map(question => {
      const type = question.Text;
      switch (type) {
        case 'City':
          setFieldValue(question.ID, place.city || '');
          break;
        case 'Zip Code':
          setFieldValue(question.ID, place.postalcode || '');
          break;
        case 'State':
          if (place.state) {
            const options = question.Options.split(',').map(option => ({
              key: option,
              value: option
            }));
            const selectedOption = options.find(option => option.value === place.state);
            if (selectedOption) {
              setFieldValue(question.ID, JSON.stringify([selectedOption]));
            }
          }
          break;
      }
    });
  };

  const handlePlaceChanged = place => {
    if (place) setFieldValue(ID, place.name);
    setAddressFields(locationObject(place));
  };

  useEffect(() => {
    if (!values[11111] && disabled) return setDisable(false);
    setDisable(true);
    setFieldValue(ID, '');
  }, [values[11111]]);

  return (
    <Box className={classes.outerWrapper}>
      <Box className={classes.fieldLabelWrapper}>
        <Typography variant="h6" className={classes.fieldLabel}>
          {Text} &nbsp;
        </Typography>
        <Typography component="span" className={typeClass}>
          {typeText}
        </Typography>
      </Box>

      <Box className={classes.textFieldOuterWrapper}>
        <Box className={classes.textFieldContainer}>
          <GoogleLocation
            handlePlaceChanged={handlePlaceChanged}
            inputProps={{
              'data-testid': `${Text}-${QuestionDataTypeId}`
            }}
            variant="outlined"
            fullWidth
            {...formik.getFieldProps(question.ID)}
            error={touched[ID] && errors[ID]}
            helperText={touched[ID] && errors[ID]}
            type="text"
            className={cs('form-control', classes.formControlClass)}
            placeholder={`Enter your ${Text}`}
            disabled={disabled}
          />
        </Box>
      </Box>
    </Box>
  );
}

export default ApiTextField;
