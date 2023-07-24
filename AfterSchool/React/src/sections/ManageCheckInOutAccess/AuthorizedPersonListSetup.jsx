import React, { useEffect, useState } from 'react';
import * as Yup from 'yup';
import { Grid, Box, Container, Typography, Divider } from '@material-ui/core';

import useStyles from './styles';
import { useFormik } from 'formik';
import { isEmpty, isEqual, toArray } from 'lodash';
import { phoneValidationRegex } from 'constants/regex';
import AuthorizedPersonListRow from './AuthorizedPersonListRow';
import AddAuthorizedUserForm from './AddAuthorizedUserForm';

const newUser = {
  ID: -1,
  FirstName: '',
  LastName: '',
  Phone: '',
  HasAccess: true,
  default: false,
  newObject: true,
  PersonPhoneDataType: 'NotSet'
};

function AuthorizedPersonListSetup({ studentName, authorizedPersons, setValue }) {
  const classes = useStyles();
  const [initialValues, setInitialValue] = useState({});
  const [validationSchema, setValidationSchema] = useState({});

  useEffect(() => {
    const initialState = {};
    authorizedPersons.forEach(
      (person, index) =>
        (initialState[index] = { HasAccess: false, ...person, default: true, newObject: false })
    );
    setInitialValue(initialState);
  }, [authorizedPersons]);

  const formValidationSchema = () =>
    Yup.object().shape(
      Object.keys(values).reduce((acc, key) => {
        acc[key] = Yup.object().shape({
          Phone: Yup.string().when(['HasAccess'], {
            is: val => val,
            then: () =>
              Yup.string()
                .matches(phoneValidationRegex, 'Phone number is not valid')
                .required('Phone Number is required'),
            otherwise: () => Yup.string().notRequired()
          }),
          FirstName: Yup.string().when(['HasAccess'], {
            is: val => val,
            then: () => Yup.string().required('First Name is required'),
            otherwise: () => Yup.string()
          }),
          LastName: Yup.string().when(['HasAccess'], {
            is: val => val,
            then: () => Yup.string().required('Last Name is required'),
            otherwise: () => Yup.string()
          })
        });

        return acc;
      }, {})
    );

  const formik = useFormik({
    enableReinitialize: true,
    initialValues,
    validationSchema
  });

  const { setFieldValue, values, errors, touched } = formik;

  useEffect(() => {
    setValidationSchema(formValidationSchema());

    const updatedValues = [];
    let shouldUpdate = true;
    Object.keys(values).forEach(key => {
      const { default: editable, newObject, ...authPerson } = values[key];
      if (!editable) shouldUpdate = false;
      updatedValues.push(authPerson);
    });
    if (!shouldUpdate) return;

    if (!isEmpty(updatedValues) && !isEqual(toArray(updatedValues), authorizedPersons))
      setValue(toArray(values));
  }, [values]);

  const addNewUserHandler = () => {
    const key = Number(Object.keys(values).at(-1)) + 1;
    setFieldValue(key, newUser);
  };

  const handleDeleteField = fieldName => {
    const { [fieldName]: deletedField, ...newValues } = formik.values;
    formik.setValues(newValues);
    formik.setFieldTouched(fieldName, false);
    formik.setFieldError(fieldName, '');
  };

  const innerContent = (val, key) => {
    if (val) {
      return (
        <AuthorizedPersonListRow
          key={key}
          person={values[key]}
          errors={errors[key]}
          touched={touched[key]}
          handleEdit={() => setFieldValue(key, { ...values[key], default: false })}
          handleAddRemoveAccess={val => setFieldValue(key, { ...values[key], HasAccess: val })}
        />
      );
    }
    return (
      <AddAuthorizedUserForm
        key={key}
        person={values[key]}
        setFieldValue={value => {
          setFieldValue(key, value);
        }}
        handleDeleteField={() => handleDeleteField(key)}
      />
    );
  };

  return (
    <Container maxWidth="1200px" className={classes.authListInnerWrapper}>
      <Grid container spacing={2} className={classes.alignLeft}>
        <Typography variant="h5" className={classes.studentName}>
          {studentName}
        </Typography>
        <Divider className={classes.divider} />
        <Box className={classes.listingWrapper}>
          {Object.keys(values).map(key => {
            return innerContent(values[key].default, key);
          })}
          <Grid item xs={12}>
            <Typography className={classes.addPersonButton} onClick={addNewUserHandler}>
              Add another authorized person
            </Typography>
          </Grid>
        </Box>
      </Grid>
    </Container>
  );
}

export default AuthorizedPersonListSetup;
