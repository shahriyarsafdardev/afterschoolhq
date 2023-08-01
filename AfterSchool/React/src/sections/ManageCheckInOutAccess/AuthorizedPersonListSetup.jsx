import React, { useEffect, useState } from 'react';
import { Grid, Box, Container, Typography, Divider } from '@material-ui/core';

import useStyles from './styles';
import { useFormik } from 'formik';
import { isEmpty, isEqual, toArray } from 'lodash';
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

function AuthorizedPersonListSetup({ studentName, authorizedPersons, setValue, studentUid }) {
  const classes = useStyles();
  const [initialValues, setInitialValue] = useState({});
  const [authorizedPersonsPhone, setAuthorizedPersonsPhone] = useState([]);

  useEffect(() => {
    const initialState = {};
    const authorizedPersonsPhoneList = [];
    authorizedPersons.forEach((person, index) => {
      authorizedPersonsPhoneList.push(person.Phone);
      initialState[index] = { HasAccess: false, ...person, default: true, newObject: false };
    });
    setInitialValue(initialState);

    setAuthorizedPersonsPhone(authorizedPersonsPhoneList);
  }, [authorizedPersons]);

  const formik = useFormik({
    enableReinitialize: true,
    initialValues
  });

  const { setFieldValue, values, errors, touched } = formik;

  useEffect(() => {
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
          key={`${key}-${studentUid}`}
          person={values[key]}
          errors={errors[key]}
          touched={touched[key]}
          studentUid={studentUid}
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
        authorizedPersonsPhoneList={authorizedPersonsPhone}
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
            <Typography
              className={classes.addPersonButton}
              onClick={addNewUserHandler}
              data-testid={`${studentName}-add-authorized-person-button`}
            >
              Add another authorized person
            </Typography>
          </Grid>
        </Box>
      </Grid>
    </Container>
  );
}

export default AuthorizedPersonListSetup;
